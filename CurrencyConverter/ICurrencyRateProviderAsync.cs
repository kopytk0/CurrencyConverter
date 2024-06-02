using System;
using System.Threading.Tasks;

namespace Jakubqwe.CurrencyConverter
{
    public interface ICurrencyRateProviderAsync : ICurrencyRateProvider
    {
        /// <summary>
        ///     Gets the conversion rate for given date asynchronously
        /// </summary>
        Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency, DateTime date);

        /// <summary>
        ///     Gets the latest conversion rate asynchronously
        /// </summary>
        Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency);
    }
}
