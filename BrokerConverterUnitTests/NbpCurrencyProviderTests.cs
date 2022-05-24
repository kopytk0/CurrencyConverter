using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrokerConverter;
using Xunit;

namespace BrokerConverterUnitTests
{
    public class NbpCurrencyProviderTests
    {
        [Fact]
        public void GetRateTest()
        {
            var provider = new NbpCurrencyProvider();
            Assert.Equal(3.7031m, provider.GetRate(new DateTime(2021, 01, 05), Currency.USD));
            Assert.Equal(3.7031m, provider.GetRate(new DateTime(2021, 01, 06), Currency.USD));
            Assert.Equal(4.06m, provider.GetRate(new DateTime(2022, 01, 01), Currency.USD));
            Assert.Equal(3.7584m, provider.GetRate(new DateTime(2021, 01, 01), Currency.USD));

            Assert.Equal(1m, provider.GetRate(new DateTime(2022, 01, 01), Currency.PLN));
        }

        [Fact]
        public void ExceptionsTest()
        {
            var provider = new NbpCurrencyProvider();
            var date = new DateTime(1, 1, 1);
            Assert.Throws<NotSupportedException>(() => provider.GetRate(date, Currency.USD, Currency.EUR));
        }

        [Fact]
        public void CanHandleTest()
        {
            var provider = new NbpCurrencyProvider();
            Assert.True(provider.CanHandle(Currency.USD, Currency.PLN));
            Assert.True(provider.CanHandle(Currency.PLN, Currency.PLN));
            Assert.False(provider.CanHandle(Currency.USD, Currency.EUR));
        }
    }
}