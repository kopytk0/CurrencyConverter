using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using CsvHelper;
using Jakubqwe.CurrencyConverter.Helpers;

namespace Jakubqwe.CurrencyConverter.CurrencyRateProviders
{
    public class EcbCurrencyProvider : ICurrencyRateProviderAsync
    {
        private static readonly Dictionary<Currency, YearlyCurrencyRates> _rates =
            new Dictionary<Currency, YearlyCurrencyRates>(); // Euro to Currency

        /// <inheritdoc />
        public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
                return true;

            return IsCurrencySupported(targetCurrency) &&
                   IsCurrencySupported(sourceCurrency); // All currencies supported by ECB
        }

        /// <inheritdoc />
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            return AsyncHelper.RunSync(() => GetRateAsync(sourceCurrency, targetCurrency, date));
        }

        /// <inheritdoc />
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency)
        {
            return AsyncHelper.RunSync(() => GetRateAsync(sourceCurrency, targetCurrency));
        }

        /// <inheritdoc />
        public bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate)
        {
            rate = default;
            if (sourceCurrency == targetCurrency)
            {
                rate = 1m;
                return true;
            }

            if (sourceCurrency != Currency.EUR)
            {
                if (!TryGetRate(Currency.EUR, sourceCurrency, date, out var eurSrcRate) ||
                    !TryGetRate(Currency.EUR, targetCurrency, date, out var eurTargetRate))
                {
                    return false;
                }

                rate = eurTargetRate / eurSrcRate;
                return true;
            }

            if (!CanHandle(sourceCurrency, targetCurrency) || DateTime.Now.Date < date)
            {
                return false;
            }

            if (!_rates.TryGetValue(targetCurrency, out var yearlyRates))
            {
                yearlyRates = new YearlyCurrencyRates(Currency.EUR, targetCurrency);
                _rates.Add(targetCurrency, yearlyRates);
            }

            if (!AsyncHelper.RunSync(() => TryCacheRate(targetCurrency, date, yearlyRates)))
            {
                return false;
            }

            return yearlyRates.TryGetRate(date, out rate);
        }

        public void ClearCache()
        {
            _rates.Clear();
        }

        private static bool IsCurrencySupported(Currency targetCurrency)
        {
            return targetCurrency <= Currency.MXN && targetCurrency != Currency.UAH && targetCurrency != Currency.CLP;
        }

        private async Task<bool> TryCacheRate(Currency targetCurrency, DateTime date, YearlyCurrencyRates yearlyRates)
        {
            if (targetCurrency == Currency.EUR) return true;

            if (!yearlyRates.ContainsRate(date))
            {
                if (!await UpdateEcbRates(date.Year, targetCurrency).ConfigureAwait(false))
                {
                    return false;
                }

                if (yearlyRates.ContainsRate(date))
                {
                    return true;
                }

                if (!await UpdateEcbRates(date.Year - 1, targetCurrency).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task EnsureRateIsCached(Currency targetCurrency, DateTime rateDate, YearlyCurrencyRates yearlyRates)
        {
            if (targetCurrency == Currency.EUR) return;

            if (await EnsureYearIsCached(targetCurrency, rateDate, rateDate.Year, yearlyRates).ConfigureAwait(false))
            {
                return;
            }

            await EnsureYearIsCached(targetCurrency, rateDate, rateDate.Year - 1, yearlyRates).ConfigureAwait(false);
        }

        private async Task<bool> EnsureYearIsCached(Currency sourceCurrency, DateTime rateDate, int year,
            YearlyCurrencyRates yearlyRates)
        {
            if (yearlyRates.ContainsRate(rateDate))
            {
                return true;
            }

            if (!await UpdateEcbRates(year, sourceCurrency))
            {
                throw new Exception($"Could not get ECB rates for year {year}");
            }

            return false;
        }

        private async Task<bool> UpdateEcbRates(int year, Currency targetCurrency)
        {
            if (!_rates.TryGetValue(targetCurrency, out var yearlyRates))
            {
                Debug.Fail($"Could not get yearly rates for currency {targetCurrency}");
                return false;
            }

            var client = new HttpClient();
            var response = await client.GetAsync(PrepareQuery(targetCurrency, Currency.EUR, year)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            yearlyRates.SetRates((ushort)year, await ParseRecords(response.Content));
            return true;
        }

        private async Task<IEnumerable<KeyValuePair<DateTime, decimal>>> ParseRecords(HttpContent content)
        {
            using (var reader = new StreamReader(await content.ReadAsStreamAsync().ConfigureAwait(false)))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<CsvConversionData>()
                        .Select(x => new KeyValuePair<DateTime, decimal>(x.TIME_PERIOD, x.OBS_VALUE)).ToList();
                }
            }
        }

        private string PrepareQuery(Currency sourceCurrency, Currency targetCurrency, int year)
        {
            return
                $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.{sourceCurrency}.EUR.SP00.A?startPeriod={year}-01-01&endPeriod={year}-12-31&format=csvdata";
        }

        private async Task<(bool Success, CurrencyRatesPair Rates)> TryGetLatestEcbRate(Currency firstCurrency, Currency secondCurrency)
        {
            if (firstCurrency == Currency.EUR)
            {
                (firstCurrency, secondCurrency) = (secondCurrency, firstCurrency);
            }
            CurrencyRatesPair rates = default;
            var client = new HttpClient();
            var response = await client.GetAsync(PrepareLatestQuery(firstCurrency, secondCurrency));
            if (!response.IsSuccessStatusCode)
            {
                return (false, rates);
            }

            using (var reader = new StreamReader(AsyncHelper.RunSync(() => response.Content.ReadAsStreamAsync())))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();

                    ParseLatestRecord(firstCurrency, ref rates, csv);
                    if (secondCurrency != Currency.EUR)
                    {
                        ParseLatestRecord(firstCurrency, ref rates, csv);
                    }
                    else
                    {
                        rates.Second = 1m;
                    }
                }
            }

            return (true, rates);
        }

        private static void ParseLatestRecord(Currency firstCurrency, ref CurrencyRatesPair result, CsvReader csv)
        {
            csv.Read();
            var data = csv.GetRecord<CsvConversionData>();
            if (data.CURRENCY == firstCurrency)
            {
                result.First = data.OBS_VALUE;
            }
            else
            {
                result.Second = data.OBS_VALUE;
            }
        }

        private string PrepareLatestQuery(Currency firstCurrency, Currency secondCurrency)
        {
            var currencyQuery = firstCurrency.ToString();
            currencyQuery += secondCurrency != Currency.EUR ? $"+{secondCurrency}" : "";

            return
                $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.{currencyQuery}.EUR.SP00.A?lastNObservations=1&format=csvdata";
        }

        /// <inheritdoc />
        public async Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            if (DateTime.Now.Date < date)
            {
                throw new ArgumentException("Date cannot be in the future");
            }

            if (sourceCurrency == targetCurrency)
            {
                return 1m;
            }

            if (sourceCurrency != Currency.EUR)
            {
                return GetRate(Currency.EUR, targetCurrency, date) / GetRate(Currency.EUR, sourceCurrency, date);
            }

            if (!CanHandle(sourceCurrency, targetCurrency))
            {
                throw
                    new NotSupportedException(
                        "Conversion not supported by ECB"); // TODO: Create CurrencyConversionNotSupportedException
            }

            if (!_rates.TryGetValue(targetCurrency, out var yearlyRates))
            {
                yearlyRates = new YearlyCurrencyRates(Currency.EUR, targetCurrency);
                _rates.Add(targetCurrency, yearlyRates);
            }

            if (!yearlyRates.ContainsRate(date))
            {
                await EnsureRateIsCached(targetCurrency, date, yearlyRates);
            }

            return yearlyRates.GetRate(date);
        }

        /// <inheritdoc />
        public async Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
            {
                return 1m;
            }

            var ratesResult = await TryGetLatestEcbRate(sourceCurrency, targetCurrency);

            if (!ratesResult.Success)
            {
                throw new Exception("Could not get rates");
            }

            if (sourceCurrency != Currency.EUR)
            {
                return ratesResult.Rates.Second / ratesResult.Rates.First;
            }

            return ratesResult.Rates.First;
        }

        internal struct CurrencyRatesPair
        {
            public decimal First;
            public decimal Second;
        }

        internal struct CsvConversionData
        {
            public DateTime TIME_PERIOD { get; set; }
            public decimal OBS_VALUE { get; set; }
            public Currency CURRENCY { get; set; }
        }
    }
}
