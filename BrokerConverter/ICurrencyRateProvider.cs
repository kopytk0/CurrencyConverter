using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerConverter
{
    public interface ICurrencyRateProvider
    {
        bool CanHandle(Currency targetCurrency, Currency sourceCurrency);
        decimal GetRate(DateTime dateCurrency, Currency targetCurrency, Currency sourceCurrency);
    }
}
