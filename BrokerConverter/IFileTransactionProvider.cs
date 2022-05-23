using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerConverter
{
    public interface IFileTransactionProvider
    {
        IEnumerable<Transaction> GetTransactions(TextReader reader);
    }
}
