using System.Collections.Generic;
using CurrencyConverter.TransactionProviders;

namespace CurrencyConverter
{
    public interface IApiTransactionProvider
    {
        IEnumerable<Transaction> GetTransactionsFromApi();
    }
}
