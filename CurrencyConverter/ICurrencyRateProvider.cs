namespace CurrencyConverter;

public interface ICurrencyRateProvider
{
    /// <summary>
    ///     Checks if conversion can be handled
    /// </summary>
    bool CanHandle(Currency sourceCurrency, Currency targetCurrency);

    /// <summary>
    ///     Gets the conversion rate
    /// </summary>
    decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date);

    /// <summary>
    ///     Tries to get the conversion rate
    /// </summary>
    bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate);
}