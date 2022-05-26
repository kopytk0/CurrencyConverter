using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace CurrencyConverter.CurrencyRateProviders;

public sealed class NbpCurrencyProvider : ICurrencyRateProvider
{
    private static readonly Dictionary<Currency, YearlyCurrencyRates> _rates = new(); // source to PLN

    /// <inheritdoc />
    public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
    {
        if (targetCurrency == sourceCurrency)
        {
            return true;
        }

        return targetCurrency <= Currency.XDR && sourceCurrency <= Currency.XDR; // All currencies supported by NBP
    }

    /// <inheritdoc />
    public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
    {
        if (DateTime.Now.Date < date)
        {
            throw new ArgumentException("Date can't be in the future");
        }

        if (targetCurrency == sourceCurrency)
        {
            return 1m;
        }

        if (!CanHandle(sourceCurrency, targetCurrency))
        {
            throw new NotSupportedException("Pair not supported by NBP");
        }

        if (targetCurrency != Currency.PLN)
        {
            return GetRate(sourceCurrency, Currency.PLN, date) / GetRate(targetCurrency, Currency.PLN, date);
        }

        _rates.TryGetValue(sourceCurrency, out var rates);
        if (rates == null)
        {
            rates = new YearlyCurrencyRates(sourceCurrency, targetCurrency);
            _rates.Add(sourceCurrency, rates);
        }

        if (!rates.ContainsYear((ushort)date.Year))
        {
            GetNbpRates(date.Year, sourceCurrency);
        }

        rates.TryGetRate(date, out var rate);

        if (rate == default)
        {
            GetNbpRates(date.Year - 1, sourceCurrency);
            rate = rates.GetRate(date);
        }

        return rate;
    }

    /// <summary>
    ///     Tries to get currency rate from the NBP api.
    /// </summary>
    public bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate)
    {
        rate = default;
        if (targetCurrency == sourceCurrency)
        {
            rate = 1m;
            return true;
        }

        if (!CanHandle(sourceCurrency, targetCurrency) || DateTime.Now.Date < date)
        {
            return false;
        }

        if (targetCurrency != Currency.PLN)
        {
            if (!TryGetRate(sourceCurrency, Currency.PLN, date, out var srcPlnRate) ||
                !TryGetRate(targetCurrency, Currency.PLN, date, out var targetPlnRate))
            {
                return false;
            }

            rate = srcPlnRate / targetPlnRate;
            return true;
        }

        _rates.TryGetValue(sourceCurrency, out var rates);
        if (rates == null)
        {
            rates = new YearlyCurrencyRates(sourceCurrency, targetCurrency);
            _rates.Add(sourceCurrency, rates);
        }

        if (!rates.ContainsYear((ushort)date.Year))
        {
            if (!TryGetNbpRates(date.Year, sourceCurrency)) return false;
        }

        rates.TryGetRate(date, out var multiplier);
        rate = multiplier;

        if (rate == default)
        {
            if (!TryGetNbpRates(date.Year - 1, sourceCurrency)) return false;
            rate = rates.GetRate(date);
        }

        return true;
    }

    public void ClearCache()
    {
        _rates.Clear();
    }

    private bool TryGetNbpRates(int year, Currency sourceCurrency)
    {
        try
        {
            GetNbpRates(year, sourceCurrency);
            return true;
        }
        catch (Exception) // failure should happen rarely so try catch is enough here
        {
            return false;
        }
    }

    private void GetNbpRates(int year, Currency sourceCurrency)
    {
        if (!_rates.TryGetValue(sourceCurrency, out var yearlyRates))
        {
            throw new Exception("No yearly rates object for currency"); // should never happen
        }

        var month = DateTime.Now.Year == year ? DateTime.Now.Month : 12;
        var day = DateTime.Now.Year == year ? DateTime.Now.Day - 1 : 31;
        var source =
            $"https://api.nbp.pl/api/exchangerates/rates/A/{sourceCurrency.ToString().ToLower()}/{year}-01-01/{year}-{month:D2}-{day:D2}/?format=json";

        var client = new HttpClient();
        var response = client.GetAsync(source).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("NbpCurrencyProvider: failed to get rates");
        }

        using (var content = response.Content.ReadAsStreamAsync().Result)
        {
            var json = JsonDocument.Parse(content);
            var jsonRates = json.RootElement.GetProperty("rates");

            var rates = jsonRates.EnumerateArray().Select(e =>
            {
                return new KeyValuePair<DateTime, decimal>(e.GetProperty("effectiveDate").GetDateTime(),
                    e.GetProperty("mid").GetDecimal());
            });

            yearlyRates.SetRates((ushort)year, rates);
        }
    }
}
