using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrokerConverter.CurrencyRateProviders;

namespace BrokerConverter
{
    /// <summary>
    /// This class is used to convert currencies
    /// </summary>
    public class CurrencyConverter
    {
        /// <summary>
        /// Provider to get rates from
        /// </summary>
        public ICurrencyRateProvider RateProvider { get; set; }

        /// <param name="rateProvider">Rate provider to get rates from</param>
        public CurrencyConverter(ICurrencyRateProvider rateProvider)
        {
            RateProvider = rateProvider;
        }

        /// <summary>
        /// Converts currencies
        /// </summary>
        public decimal Convert(Currency sourceCurrency, Currency targetCurrency, DateTime date, decimal amount)
        {
            var rate = RateProvider.GetRate(date, sourceCurrency, targetCurrency);
            return amount * rate;
        }
    }
}
