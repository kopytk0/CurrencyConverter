using System.Collections.Generic;
using Jakubqwe.CurrencyConverter.TransactionProviders;

namespace Jakubqwe.CurrencyConverter
{
    public interface IApiTransactionProvider
    {
        IEnumerable<Transaction> GetTransactionsFromApi();
    }
}
