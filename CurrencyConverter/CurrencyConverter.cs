using System;
using System.Threading.Tasks;

namespace Jakubqwe.CurrencyConverter
{
    /// <summary>
    ///     This class is used to convert currencies
    /// </summary>
    public class CurrencyConverter
    {
        /// <param name="rateProvider">Rate provider to get rates from</param>
        public CurrencyConverter(ICurrencyRateProvider rateProvider)
        {
            RateProvider = rateProvider;
        }

        /// <summary>
        ///     Provider to get rates from
        /// </summary>
        public virtual ICurrencyRateProvider RateProvider { get; set; }

        public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
        {
            if (sourceCurrency == targetCurrency)
            {
                return true;
            }

            return RateProvider.CanHandle(sourceCurrency, targetCurrency);
        }

        /// <summary>
        ///     Converts currencies using latest rate
        /// </summary>
        public decimal Convert(Currency sourceCurrency, Currency targetCurrency, decimal amount)
        {
            if (sourceCurrency == targetCurrency)
            {
                return amount;
            }

            return amount * RateProvider.GetRate(sourceCurrency, targetCurrency);
        }

        /// <summary>
        ///     Converts currencies using rate from specified date
        /// </summary>
        public decimal Convert(Currency sourceCurrency, Currency targetCurrency, DateTime date, decimal amount)
        {
            if (sourceCurrency == targetCurrency)
            {
                return amount;
            }

            return amount * RateProvider.GetRate(sourceCurrency, targetCurrency, date);
        }

        /// <summary>
        ///     Converts currencies using rate from specified date asynchronously, if the rate provider supports it.
        ///     Supported providers - EcbCurrencyProvider
        /// </summary>
        public async Task<decimal> ConvertAsync(Currency sourceCurrency, Currency targetCurrency, DateTime date, decimal amount)
        {
            if(!(RateProvider is ICurrencyRateProviderAsync asyncRateProvider))
            {
                throw new NotSupportedException($"Rate provider doesn't {nameof(RateProvider)} support async conversion");
            }

            if (sourceCurrency == targetCurrency)
            {
                return amount;
            }

            return amount * await asyncRateProvider.GetRateAsync(sourceCurrency, targetCurrency, date).ConfigureAwait(false);
        }

        /// <summary>
        ///     Converts currencies using the newest rate asynchronously, if the rate provider supports it.
        ///     Supported providers - EcbCurrencyProvider
        /// </summary>
        public async Task<decimal> ConvertAsync(Currency sourceCurrency, Currency targetCurrency, decimal amount)
        {
            if (!(RateProvider is ICurrencyRateProviderAsync asyncRateProvider))
            {
                throw new NotSupportedException($"Rate provider doesn't {nameof(RateProvider)} support async conversion");
            }

            if (sourceCurrency == targetCurrency)
            {
                return amount;
            }

            return amount * await asyncRateProvider.GetRateAsync(sourceCurrency, targetCurrency).ConfigureAwait(false);
        }

        /// <summary>
        ///     Tries to convert currencies using rate from specified date
        /// </summary>
        public bool TryConvert(Currency sourceCurrency, Currency targetCurrency, DateTime date, decimal amount,
            out decimal result)
        {
            if (sourceCurrency == targetCurrency)
            {
                result = amount;
                return true;
            }

            if (!RateProvider.TryGetRate(sourceCurrency, targetCurrency, date, out var rate))
            {
                result = default;
                return false;
            }

            result = amount * rate;
            return true;
        }
    }
}
