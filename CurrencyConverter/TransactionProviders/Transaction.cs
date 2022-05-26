using System;

namespace CurrencyConverter.TransactionProviders;

public struct Transaction
{
    public Currency BaseCurrency;
    public decimal Income;
    public DateTime Date;
}
