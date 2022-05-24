using BrokerConverter.TransactionProviders;

namespace BrokerConverter;

public interface IApiTransactionProvider
{
    IEnumerable<Transaction> GetTransactionsFromApi();
}