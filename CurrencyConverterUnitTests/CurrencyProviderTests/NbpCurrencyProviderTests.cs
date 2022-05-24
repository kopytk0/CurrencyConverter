using System;
using CurrencyConverter;
using CurrencyConverter.CurrencyRateProviders;
using Xunit;

namespace CurrencyConverterUnitTests;

public class NbpCurrencyProviderTests
{
    [Fact]
    public void GetRateTest()
    {
        var provider = NbpCurrencyProvider.Instance;
        Assert.Equal(3.7031m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05)));
        Assert.Equal(3.7031m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 06)));
        Assert.Equal(4.06m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2022, 01, 01)));
        Assert.Equal(3.7584m, provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 01)));

        Assert.Equal(1m, provider.GetRate(Currency.PLN, Currency.PLN, new DateTime(2021, 01, 01)));
    }

    [Fact]
    public void TryGetRateTest()
    {
        var provider = NbpCurrencyProvider.Instance;

        Assert.True(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05), out var rate));
        Assert.Equal(3.7031m, rate);
        Assert.True(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2019, 01, 01), out rate));
        Assert.Equal(3.7597m, rate);
        Assert.True(provider.TryGetRate(Currency.PLN, Currency.PLN, new DateTime(2021, 01, 01), out rate));
        Assert.Equal(1m, rate);

        Assert.False(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2023, 01, 05), out rate));
        Assert.Equal(default, rate);
    }

    [Fact]
    public void ExceptionsTest()
    {
        var provider = NbpCurrencyProvider.Instance;
        var date = new DateTime(1, 1, 1);
        Assert.Throws<NotSupportedException>(() => provider.GetRate(Currency.USD, Currency.EUR, date));
        Assert.Throws<ArgumentException>(() => provider.GetRate(Currency.USD, Currency.EUR, DateTime.Now.AddDays(1)));
    }

    [Fact]
    public void CanHandleTest()
    {
        var provider = NbpCurrencyProvider.Instance;
        Assert.True(provider.CanHandle(Currency.USD, Currency.PLN));
        Assert.True(provider.CanHandle(Currency.PLN, Currency.PLN));
        Assert.False(provider.CanHandle(Currency.USD, Currency.EUR));
    }
}
