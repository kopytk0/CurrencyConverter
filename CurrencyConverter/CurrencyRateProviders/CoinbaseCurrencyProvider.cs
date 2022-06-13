using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using Jakubqwe.CurrencyConverter.Helpers;

namespace Jakubqwe.CurrencyConverter.CurrencyRateProviders
{
    /// <summary>
    ///     Provides currency rates from Coinbase.
    ///     Only real time rates are supported.
    /// </summary>
    public class CoinbaseCurrencyProvider : ICurrencyRateProvider
    {
        /// <inheritdoc />
        public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
        {
            return true;
        }

        /// <summary>
        ///     Historical rates are not supported. Throws <see cref="NotSupportedException" />.
        /// </summary>
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            throw new NotSupportedException("Coinbase does not support historical rates");
        }

        /// <inheritdoc />
        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
            {
                return 1m;
            }

            var query = $"https://api.coinbase.com/v2/exchange-rates?currency={sourceCurrency}";
            var client = new HttpClient();
            var response = client.GetAsync(query).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to get rate from Coinbase");
            }

            using (var messageStream = AsyncHelper.RunSync(() => response.Content.ReadAsStreamAsync()))
            {
                var json = JsonDocument.Parse(messageStream);
                var rate = json.RootElement.GetProperty("data").GetProperty("rates")
                               .GetProperty(targetCurrency.ToString()).GetString() ??
                           throw new Exception("Failed to parse rate from Coinbase");
                return Decimal.Parse(rate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        ///     Historical rates are not supported. Throws <see cref="NotSupportedException" />.
        /// </summary>
        public bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate)
        {
            throw new NotSupportedException("Coinbase does not support historical rates");
        }
    }
}
