using System;

namespace CurrencyConverter;

public interface ICurrencyRateProvider // TODO: GetRate for latest data
{
    /// <summary>
    ///     Checks if conversion can be handled
    /// </summary>
    bool CanHandle(Currency sourceCurrency, Currency targetCurrency);

    /// <summary>
    ///     Gets the conversion rate for given date
    /// </summary>
    decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date);

    /// <summary>
    ///     Tries to get the conversion rate
    /// </summary>
    bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate);

    void ClearCache();
}
