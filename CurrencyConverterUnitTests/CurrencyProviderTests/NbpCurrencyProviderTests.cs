using System;
using Jakubqwe.CurrencyConverter;
using Jakubqwe.CurrencyConverter.CurrencyRateProviders;
using Xunit;

namespace CurrencyConverterUnitTests;

public class NbpCurrencyProviderTests
{
    [Fact]
    public void GetRateTest()
    {
        var provider = new NbpCurrencyProvider();
        Assert.Equal(3.7031m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05)));
        Assert.Equal(3.7031m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 06)));
        Assert.Equal(4.06m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2022, 01, 01)));
        Assert.Equal(3.7584m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 01)));

        Assert.Equal(1m, provider.GetRate(Currency.PLN, Currency.PLN, new DateTime(2021, 01, 01)));

        Assert.Equal(0.27m, Decimal.Round(provider.GetRate(Currency.PLN, Currency.USD, new DateTime(2021, 01, 05)), 4));
        Assert.Equal(1.2272m, Decimal.Round(provider.GetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 05)), 4));
    }

    [Fact]
    public void GetLatestRateTest()
    {
        var provider = new NbpCurrencyProvider();
        Assert.True(provider.GetRate(Currency.USD, Currency.PLN) > 1);
        Assert.True(provider.GetRate(Currency.PLN, Currency.USD) < 1);
        Assert.True(provider.GetRate(Currency.USD, Currency.CZK) > 1);
        Assert.True(provider.GetRate(Currency.CZK, Currency.USD) < 1);

        Assert.Equal(1m, provider.GetRate(Currency.PLN, Currency.PLN));
    }

    [Fact]
    public void TryGetRateTest()
    {
        var provider = new NbpCurrencyProvider();

        Assert.True(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05), out var rate));
        Assert.Equal(3.7031m, rate);
        Assert.True(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2019, 01, 01), out rate));
        Assert.Equal(3.7597m, rate);
        Assert.True(provider.TryGetRate(Currency.PLN, Currency.PLN, new DateTime(2021, 01, 01), out rate));
        Assert.Equal(1m, rate);

        Assert.False(provider.TryGetRate(Currency.USD, Currency.PLN, DateTime.Today.AddDays(1), out rate));
        Assert.Equal(default, rate);

        Assert.True(provider.TryGetRate(Currency.PLN, Currency.USD, new DateTime(2021, 01, 05), out rate));
        Assert.Equal(0.27m, Decimal.Round(rate, 4));
        Assert.True(provider.TryGetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 05), out rate));
        Assert.Equal(1.2272m, Decimal.Round(rate, 4));
    }

    [Fact]
    public void ExceptionsTest()
    {
        var provider = new NbpCurrencyProvider();
        Assert.Throws<ArgumentException>(() => provider.GetRate(Currency.USD, Currency.EUR, DateTime.Now.AddDays(1)));
    }

    [Fact]
    public void CanHandleTest()
    {
        var provider = new NbpCurrencyProvider();
        Assert.True(provider.CanHandle(Currency.USD, Currency.PLN));
        Assert.True(provider.CanHandle(Currency.PLN, Currency.PLN));
        Assert.True(provider.CanHandle(Currency.USD, Currency.EUR));
    }
}
