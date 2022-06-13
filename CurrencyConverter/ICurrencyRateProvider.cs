using System;

namespace Jakubqwe.CurrencyConverter
{
    public interface ICurrencyRateProvider
    {
        /// <summary>
        ///     Checks if conversion can be handled
        /// </summary>
        bool CanHandle(Currency sourceCurrency, Currency targetCurrency);

        /// <summary>
        ///     Gets the conversion rate for given date
        /// </summary>
        decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date);

        /// <summary>
        ///     Gets the latest conversion rate
        /// </summary>
        decimal GetRate(Currency sourceCurrency, Currency targetCurrency);

        /// <summary>
        ///     Tries to get the conversion rate for given date
        /// </summary>
        bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate);
    }
}
