using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrokerConverter.TransactionProviders;

namespace BrokerConverter
{
    public interface IApiTransactionProvider
    {
        IEnumerable<Transaction> GetTransactionsFromApi();
    }
}