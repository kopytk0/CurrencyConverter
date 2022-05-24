using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerConverter
{
    public interface ICurrencyRateProvider
    {
        bool CanHandle(Currency sourceCurrency, Currency targetCurrency);
        decimal GetRate(DateTime date, Currency sourceCurrency, Currency targetCurrency);
    }
}