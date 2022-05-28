using System;
using System.Collections.Generic;

namespace Jakubqwe.CurrencyConverter
{
    internal interface ICurrencyRates
    {
        Currency TargetCurrency { get; }
        Currency SourceCurrency { get; }
        decimal GetRate(DateTime date);
        bool TryGetRate(DateTime date, out decimal rate);
        bool ContainsRate(DateTime date);
        void SetRates(ushort year, IEnumerable<KeyValuePair<DateTime, decimal>> rates);
    }
}
