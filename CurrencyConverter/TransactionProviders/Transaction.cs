using System;

namespace Jakubqwe.CurrencyConverter.TransactionProviders
{
    public struct Transaction
    {
        public Currency BaseCurrency;
        public decimal Income;
        public DateTime Date;
    }
}
