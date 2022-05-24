using BrokerConverter.TransactionProviders;

namespace BrokerConverter;

public interface IFileTransactionProvider
{
    IEnumerable<Transaction> GetTransactions(TextReader reader);
}