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
    public class EcbCurrencyProvider : ICurrencyRateProvider
    {
        private readonly Dictionary<Currency, YearlyCurrencyRates> _rates;

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
                   sourceCurrency is <= (Currency)33 and not Currency.UAH and not Currency.CLP;
        }
        
        /// <inheritdoc/>
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            if (DateTime.Now.Date < date)
            {
                throw new ArgumentException("Date can't be in the future");
            }

            if (sourceCurrency == targetCurrency)
            {
                return 1m;
            }

            if (!CanHandle(sourceCurrency, targetCurrency))
            {
                throw new NotSupportedException("Conversion not supported by ECB");
            }


            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                yearlyRates = new YearlyCurrencyRates(sourceCurrency, targetCurrency);
                _rates.Add(sourceCurrency, yearlyRates);
            }

            if (!yearlyRates.TryGetRate(date, out var rate))
            {
                if(!TryGetEcbRates(date.Year, sourceCurrency))
                    throw new Exception($"Could not get ECB rates for year {date.Year}");
            }
            if (rate == default)
            {
                if(!TryGetEcbRates(date.Year - 1, sourceCurrency))
                    throw new Exception($"Could not get ECB rates for year {date.Year - 1}");
                rate = yearlyRates.GetRate(date);
            }

            return rate;
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
            }

            yearlyRates.TryGetRate(date, out rate);
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

            return true;
        }
        private bool TryGetEcbRates(int year, Currency sourceCurrency)
        {
            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                throw new InvalidDataException("Rates not found"); // this should never happen
            }

            var query = $"https://sdw-wsrest.ecb.europa.eu/service/data/EXR/D.{sourceCurrency}.EUR.SP00.A?startPeriod={year}-01-01&endPeriod={year}-12-31&format=csvdata";
            var client = new HttpClient();

            var response = client.GetAsync(query).Result;
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            using var stream = response.Content.ReadAsStream();
            using var csv = new CsvReader(new StreamReader(stream), CultureInfo.InvariantCulture);
            var records = csv.GetRecords<CsvTransactionData>()
                .Select((x) => new KeyValuePair<DateTime,decimal>(x.TIME_PERIOD, x.OBS_VALUE));

            yearlyRates.SetRates((ushort)year, records);
            return true;
        }

        internal struct CsvTransactionData
        {
            public DateTime TIME_PERIOD { get; set; }
            public decimal OBS_VALUE { get; set; }
        }
          
    }
}