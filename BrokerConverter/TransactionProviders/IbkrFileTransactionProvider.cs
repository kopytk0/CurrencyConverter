
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace BrokerConverter.TransactionProviders
{
    /// <summary>
    /// This class provides transactions from IBKR from summary in CSV format
    /// </summary>
    public class IbkrFileTransactionProvider : IFileTransactionProvider
    {
        /// <summary>
        /// Returns transactions from Realized Summary CSV
        /// </summary>
        /// <param name="csvTextReader">Text reader of csv</param>
        /// <returns></returns>
        public IEnumerable<Transaction> GetTransactions(TextReader csvTextReader)
        {
            using var csv = new CsvReader(csvTextReader, new CsvConfiguration(CultureInfo.InvariantCulture));

            while (csv.Read())
            {
                if (csv.GetField<string>(0) != "Trades")
                {
                    continue;
                }
                
                if (csv.GetField<string>(1) == "Header")
                {
                    csv.ReadHeader();
                    continue;
                }

                if (csv.GetField<string>("Header") != "Data")
                {
                    continue;
                }

                if (!csv.HeaderRecord.Contains("Realized P/L"))
                { 
                    continue;
                }

                var transaction = new Transaction
                {
                    BaseCurrency = csv.GetField<Currency>("Currency"),
                    Income = csv.GetField<decimal>("Realized P/L"),
                    Date = csv.GetField<DateTime>("Date/Time")
                };

                yield return transaction;
            }

        }

        /// <summary>
        /// Returns transactions from summary CSV file
        /// </summary>
        /// <param name="csvFilePath">Path of the csv file</param>
        /// <returns></returns>
        public IEnumerable<Transaction> GetTransactions(string csvFilePath)
        {
            using var reader = new StreamReader(csvFilePath);
            return GetTransactions(reader);
        }
    }
}