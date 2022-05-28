using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jakubqwe.CurrencyConverter;
using Jakubqwe.CurrencyConverter.CurrencyRateProviders;
using Xunit;

namespace CurrencyConverterUnitTests.CurrencyProviderTests
{
    public class CoinbaseCurrencyProviderTests
    {
        [Fact]
        public void GetRateTest()
        {
            var provider = new CoinbaseCurrencyProvider();
            var rate = provider.GetRate(Currency.USD, Currency.PLN);
            Assert.True(rate > 1);
            Assert.Equal(1, provider.GetRate(Currency.USD, Currency.USD));
        }

        [Fact]
        public void CanHandleTest()
        {
            var provider = new CoinbaseCurrencyProvider();
            Assert.True(provider.CanHandle(Currency.USD, Currency.BTC));
        }
    }
}
