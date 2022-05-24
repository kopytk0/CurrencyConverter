using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace CurrencyConverter.CurrencyRateProviders
{
    public sealed class EcbCurrencyProvider : ICurrencyRateProvider
    {
        private readonly Dictionary<Currency, YearlyCurrencyRates> _rates; // TODO: change singleton into resolver

        private static readonly Lazy<EcbCurrencyProvider> _instance = new Lazy<EcbCurrencyProvider>(() => new EcbCurrencyProvider());
        public static EcbCurrencyProvider Instance => _instance.Value;

        private EcbCurrencyProvider()
        {
            _rates = new Dictionary<Currency, YearlyCurrencyRates>();
        }

        /// <inheritdoc/>
        public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
                return true;

            return targetCurrency == Currency.EUR &&
                   sourceCurrency is <= Currency.MXN and not Currency.UAH and not Currency.CLP; // All currencies supported by ECB
        }

        /// <inheritdoc/>
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            if (DateTime.Now.Date < date)
            {
                throw new ArgumentException("Date cannot be in the future");
            }

            if (sourceCurrency == targetCurrency)
            {
                return 1m;
            }

            if (!CanHandle(sourceCurrency, targetCurrency))
            {
                throw new NotSupportedException("Conversion not supported by ECB"); // TODO: Create CurrencyConversionNotSupportedException
            }

            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                yearlyRates = new YearlyCurrencyRates(sourceCurrency, Currency.EUR);
                _rates.Add(sourceCurrency, yearlyRates);
            }

            if (yearlyRates.TryGetRate(date, out var rate))
            {
                return rate;
            }

            EnsureRateIsCached(sourceCurrency, date, yearlyRates);

            rate = yearlyRates.GetRate(date);
           

            return rate;
        }

        private void EnsureRateIsCached(Currency sourceCurrency, DateTime date, YearlyCurrencyRates yearlyRates)
        {
            if (!TryGetEcbRates(date.Year, sourceCurrency))
            {
                throw new Exception($"Could not get ECB rates for year {date.Year}");
            }

            if (yearlyRates.ContainsRate(date))
            {
                return;
            }

            if (!TryGetEcbRates(date.Year - 1, sourceCurrency))
            {
                throw new Exception($"Could not get ECB rates for year {date.Year - 1}");
            }
        }

        /// <inheritdoc/>
        public bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate)
        {
            rate = default;
            if (DateTime.Now.Date < date)
            {
                return false;
            }
            
            if (sourceCurrency == targetCurrency)
            {
                rate = 1m;
                return true;
            }

            if (!CanHandle(sourceCurrency, targetCurrency))
            {
                return false;
            }

            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                yearlyRates = new YearlyCurrencyRates(sourceCurrency, targetCurrency);
                _rates.Add(sourceCurrency, yearlyRates);
            }

            if (!yearlyRates.ContainsYear((ushort)date.Year))
            {
                if (!TryGetEcbRates(date.Year, sourceCurrency))
                {
                    return false;
                }
                if (rate == default)
                {
                    if (!TryGetEcbRates(date.Year - 1, sourceCurrency))
                    {
                        return false;
                    }
                    if (!yearlyRates.TryGetRate(date, out rate))
                    {
                        return false;
                    }
                }
            }

            yearlyRates.TryGetRate(date, out rate);

            return true;
        }
        private bool TryGetEcbRates(int year, Currency sourceCurrency)
        {
            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                throw new InvalidDataException("Rates not found"); // this should never happen
            }

            var client = new HttpClient();
            var response = client.GetAsync(PrepareQuery(sourceCurrency, Currency.EUR, year)).Result;
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            yearlyRates.SetRates((ushort)year, ParseRecords(response.Content));
            return true;
        }

        private IEnumerable<KeyValuePair<DateTime, decimal>> ParseRecords(HttpContent content)
        {
            using (var reader = new StreamReader(content.ReadAsStream()))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    return csv.GetRecords<CsvConversionData>()
                        .Select((x) => new KeyValuePair<DateTime, decimal>(x.TIME_PERIOD, x.OBS_VALUE));
                }
            }
            
        }


        private string PrepareQuery(Currency sourceCurrency, Currency targetCurrency, int year)
        {
            return $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.{sourceCurrency}.EUR.SP00.A?startPeriod={year}-01-01&endPeriod={year}-12-31&format=csvdata";
        }

        internal struct CsvConversionData
        {
            public DateTime TIME_PERIOD { get; set; }
            public decimal OBS_VALUE { get; set; }
        }
          
    }
}