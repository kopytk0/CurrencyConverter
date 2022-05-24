using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerConverter.TransactionProviders
{
    public struct Transaction
    {
        public Currency BaseCurrency;
        public decimal Income;
        public DateTime Date;
    }
}