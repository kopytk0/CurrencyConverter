using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace BrokerConverter
{
    internal class NbpCurrencyProvider : ICurrencyRateProvider
    {
        private readonly Dictionary<Currency, YearlyCurrencyRates> _rates; // source to PLN

        public NbpCurrencyProvider()
        {
            _rates = new Dictionary<Currency, YearlyCurrencyRates>();
        }

        public bool CanHandle(Currency targetCurrency, Currency sourceCurrency)
        {
            if (targetCurrency == sourceCurrency)
            {
                return false;
            }
            
            return targetCurrency == Currency.PLN && sourceCurrency <= (Currency)33;
        }

        public decimal GetRate(DateTime date, Currency sourceCurrency, Currency targetCurrency = Currency.PLN)
        {
            if (targetCurrency != Currency.PLN)
            {
                throw new NotSupportedException("NBP api only supports conversion to PLN");
            }

            if (sourceCurrency == targetCurrency)
                return 1;

            _rates.TryGetValue(sourceCurrency, out var rates);
            if (rates == null)
            {
                rates = new YearlyCurrencyRates(sourceCurrency, targetCurrency);
                _rates.Add(sourceCurrency, rates);
            }
            
            if (!rates.ContainsYear((ushort)date.Year))
            {
                GetNbpRates(date.Year, sourceCurrency);
            }

            rates.TryGetRate(date, out var rate);

            if (rate == default)
            {
                GetNbpRates(date.Year - 1, sourceCurrency);
                rate = rates.GetRate(date);
            }

            return rate;
        }

        private void GetNbpRates(int year, Currency sourceCurrency)
        {
            var month = DateTime.Now.Year == year ? DateTime.Now.Month : 12;
            var day = DateTime.Now.Year == year ? DateTime.Now.Day - 1 : 31;
            var source =
                $"https://api.nbp.pl/api/exchangerates/rates/A/{sourceCurrency.ToString().ToLower()}/{year}-01-01/{year}-{month.ToString("D2")}-{day.ToString("D2")}/?format=json";

            var client = new HttpClient();
            var response = client.GetAsync(source).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("NbpCurrencyProvider: failed to get rates");
            }

            using var content = response.Content.ReadAsStream();
            var json = System.Text.Json.JsonDocument.Parse(content);
            var jsonRates = json.RootElement.GetProperty("rates");

            var rates = jsonRates.EnumerateArray().Select(e =>
            {
                return new KeyValuePair<DateTime, decimal>(e.GetProperty("effectiveDate").GetDateTime(), e.GetProperty("mid").GetDecimal());
            });

            if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
            {
                throw new Exception("No yearly rates object for currency");
            }
            yearlyRates.SetRates((ushort)year, rates);

        }
    }
}
