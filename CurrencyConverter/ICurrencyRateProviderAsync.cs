using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jakubqwe.CurrencyConverter
{
    internal interface ICurrencyRateProviderAsync : ICurrencyRateProvider
    {
        Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency, DateTime date);

        Task<decimal> GetRateAsync(Currency sourceCurrency, Currency targetCurrency);
    }
}
