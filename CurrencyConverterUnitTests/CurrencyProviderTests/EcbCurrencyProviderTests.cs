using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jakubqwe.CurrencyConverter;
using Jakubqwe.CurrencyConverter.CurrencyRateProviders;
using Xunit;

namespace CurrencyConverterUnitTests
{
    public class EcbCurrencyProviderTests
    {
        [Fact]
        public void GetRateTest()
        {
            var provider = new EcbCurrencyProvider();
            Assert.Equal(1.2271m, provider.GetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 05)));
            Assert.Equal(1.2338m, provider.GetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 06)));
            Assert.Equal(1.1326m, provider.GetRate(Currency.EUR, Currency.USD, new DateTime(2022, 01, 01)));
            Assert.Equal(1.2271m, provider.GetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 01)));

            Assert.Equal(1m, provider.GetRate(Currency.EUR, Currency.EUR, new DateTime(2021, 01, 01)));

            Assert.Equal(3.7057m, Decimal.Round(provider.GetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05)), 4));
            Assert.Equal(0.8149m, Decimal.Round(provider.GetRate(Currency.USD, Currency.EUR, new DateTime(2021, 01, 05)), 4));
        }

        [Fact]
        public void TryGetRateTest()
        {
            var provider = new EcbCurrencyProvider();

            Assert.True(provider.TryGetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 05), out var rate));
            Assert.Equal(1.2271m, rate);
            Assert.True(provider.TryGetRate(Currency.EUR, Currency.USD, new DateTime(2019, 01, 01), out rate));
            Assert.Equal(1.145m, rate);
            Assert.True(provider.TryGetRate(Currency.EUR, Currency.EUR, new DateTime(2020, 01, 01), out rate));
            Assert.Equal(1m, rate);

            Assert.False(provider.TryGetRate(Currency.USD, Currency.EUR, DateTime.Today.AddDays(1), out rate));
            Assert.Equal(default, rate);

            Assert.True(provider.TryGetRate(Currency.USD, Currency.PLN, new DateTime(2021, 01, 05), out rate));
            Assert.Equal(3.7057m, Decimal.Round(rate, 4));

            Assert.True(provider.TryGetRate(Currency.EUR, Currency.USD, new DateTime(2021, 01, 05), out rate));
            Assert.Equal(1.2271m, Decimal.Round(rate, 4));
        }

        [Fact]
        public void GetLatestRateTest()
        {
            var provider = new EcbCurrencyProvider();

            Assert.True(provider.GetRate(Currency.USD, Currency.CZK) > 1);
            Assert.True(provider.GetRate(Currency.CZK, Currency.USD) < 1);
            Assert.True(provider.GetRate(Currency.CZK, Currency.EUR) < 1);
            Assert.True(provider.GetRate(Currency.EUR, Currency.CZK) > 1);
            Assert.Equal(1m, provider.GetRate(Currency.EUR, Currency.EUR));
        }

        [Fact]
        public void ExceptionsTest()
        {
            var provider = new EcbCurrencyProvider();
            Assert.Throws<ArgumentException>(() => provider.GetRate(Currency.USD, Currency.EUR, DateTime.Now.AddDays(1)));
        }

        [Fact]
        public void CanHandleTest()
        {
            var provider = new EcbCurrencyProvider();
            Assert.True(provider.CanHandle(Currency.USD, Currency.EUR));
            Assert.True(provider.CanHandle(Currency.PLN, Currency.PLN));
            Assert.True(provider.CanHandle(Currency.USD, Currency.MXN));
        }
    }
}
