using System;
using Jakubqwe.CurrencyConverter;
using Xunit;

namespace CurrencyConverterUnitTests;

public class CurrencyConverterTests
{
    [Fact]
    public void ConvertCurrenciesTest()
    {
        var converter = new CurrencyConverter(new CurrencyRateProviderMock());

        Assert.Equal(10m, converter.Convert(Currency.USD, Currency.EUR, new DateTime(9, 9, 10), 1m));
        Assert.Equal(1m, converter.Convert(Currency.EUR, Currency.EUR, new DateTime(9, 9, 10), 1m));
        Assert.Throws<Exception>(() => converter.Convert(Currency.EUR, Currency.USD, new DateTime(9, 9, 10), 1m));
    }

    [Fact]
    public void TryConvertCurrenciesTest()
    {
        var converter = new CurrencyConverter(new CurrencyRateProviderMock());

        Assert.True(converter.TryConvert(Currency.USD, Currency.EUR, new DateTime(9, 9, 10), 1, out var result));
        Assert.Equal(10m, result);
        Assert.True(converter.TryConvert(Currency.USD, Currency.USD, new DateTime(9, 9, 10), 1, out result));
        Assert.Equal(1m, result);

        Assert.False(converter.TryConvert(Currency.EUR, Currency.USD, new DateTime(9, 9, 10), 1, out result));
        Assert.Equal(default, result);
    }

    [Fact]
    public void CanHandleTest()
    {
        var converter = new CurrencyConverter(new CurrencyRateProviderMock());

        Assert.True(converter.CanHandle(Currency.USD, Currency.EUR));
        Assert.True(converter.CanHandle(Currency.USD, Currency.USD));
        Assert.False(converter.CanHandle(Currency.EUR, Currency.USD));
    }

    internal class CurrencyRateProviderMock : ICurrencyRateProvider
    {
        public bool CanHandle(Currency sourceCurrency, Currency targetCurrency)
        {
            if (targetCurrency == Currency.EUR)
            {
                return true;
            }

            return false;
        }

        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date)
        {
            if (targetCurrency == Currency.EUR)
            {
                return date.Day;
            }

            throw new Exception();
        }

        public decimal GetRate(Currency sourceCurrency, Currency targetCurrency)
        {
            throw new NotImplementedException();
        }

        public bool TryGetRate(Currency sourceCurrency, Currency targetCurrency, DateTime date, out decimal rate)
        {
            try
            {
                rate = GetRate(sourceCurrency, targetCurrency, date);
                return true;
            }
            catch (Exception)
            {
                rate = default;
                return false;
            }
        }

        public void ClearCache()
        {
            throw new NotImplementedException();
        }
    }
}
