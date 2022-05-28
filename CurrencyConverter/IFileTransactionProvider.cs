using System.Collections.Generic;
using System.IO;
using Jakubqwe.CurrencyConverter.TransactionProviders;

namespace Jakubqwe.CurrencyConverter
{
    public interface IFileTransactionProvider
    {
        IEnumerable<Transaction> GetTransactions(TextReader reader);
    }
}
