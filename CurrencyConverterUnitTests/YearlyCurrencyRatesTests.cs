using System;
using System.Collections.Generic;
using CurrencyConverter;
using Xunit;

namespace CurrencyConverterUnitTests;

public class YearlyCurrencyRatesTests
{
    internal YearlyCurrencyRates GetSampleRates()
    {
        var ratesList = new List<KeyValuePair<DateTime, decimal>>
        {
            new(new DateTime(5, 12, 29), 1.17m),
            new(new DateTime(5, 12, 15), 1.18m),
            new(new DateTime(5, 02, 1), 1.13m)
        };
        var rates = new YearlyCurrencyRates(Currency.EUR, Currency.USD, 5, ratesList);
        ratesList = new List<KeyValuePair<DateTime, decimal>>
        {
            new(new DateTime(4, 12, 31), 2m)
        };
        rates.SetRates(4, ratesList);
        return rates;
    }

    [Fact]
    public void TestGetRate()
    {
        var rates = GetSampleRates();

        Assert.Equal(1.17m, rates.GetRate(new DateTime(5, 12, 29)));
        Assert.Equal(1.18m, rates.GetRate(new DateTime(5, 12, 15)));
        Assert.Equal(1.13m, rates.GetRate(new DateTime(5, 02, 1)));

        Assert.Equal(1.17m, rates.GetRate(new DateTime(5, 12, 31)));
        Assert.Equal(2m, rates.GetRate(new DateTime(5, 01, 30)));

        Assert.False(rates.TryGetRate(new DateTime(4, 1, 1), out var rate));
        Assert.False(rates.TryGetRate(new DateTime(3, 1, 1), out rate));
        Assert.Equal(0m, rate);
        Assert.True(rates.TryGetRate(new DateTime(5, 1, 1), out rate));
        Assert.Equal(2m, rate);

        Assert.Equal(Currency.USD, rates.TargetCurrency);
        Assert.Equal(Currency.EUR, rates.SourceCurrency);
    }

    [Fact]
    public void TestContainsRate()
    {
        var rates = GetSampleRates();
        Assert.True(rates.ContainsRate(new DateTime(5, 12, 29)));
        Assert.True(rates.ContainsYear(5));

        Assert.True(rates.ContainsRate(new DateTime(5, 12, 31)));
        Assert.True(rates.ContainsRate(new DateTime(5, 01, 30)));

        Assert.False(rates.ContainsRate(new DateTime(4, 12, 30)));
        Assert.False(rates.ContainsRate(new DateTime(42, 12, 30)));
    }

    [Fact]
    public void TestExceptions()
    {
        Assert.Throws<ArgumentException>(() => new YearlyCurrencyRates(Currency.USD, Currency.USD));

        var rates = GetSampleRates();
        Assert.Throws<KeyNotFoundException>(() => rates.GetRate(new DateTime(4, 1, 1)));
        Assert.Throws<KeyNotFoundException>(() => rates.GetRate(new DateTime(42, 1, 1)));
    }
}