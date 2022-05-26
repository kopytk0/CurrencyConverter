using System.Collections.Generic;
using System.IO;
using CurrencyConverter.TransactionProviders;

namespace CurrencyConverter
{

    public interface IFileTransactionProvider
    {
        IEnumerable<Transaction> GetTransactions(TextReader reader);
    }
}
