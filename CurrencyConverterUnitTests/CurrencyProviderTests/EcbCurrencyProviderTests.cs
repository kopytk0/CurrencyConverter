﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyConverter;
using CurrencyConverter.CurrencyRateProviders;
using Xunit;

namespace CurrencyConverterUnitTests
{
    public class EcbCurrencyProviderTests
    {
        [Fact]
        public void GetRateTest()
        {
            var provider = EcbCurrencyProvider.Instance;
            Assert.Equal(1.2271m, provider.GetRate(Currency.USD, Currency.EUR, new DateTime(2021, 01, 05)));
            Assert.Equal(1.2338m, provider.GetRate(Currency.USD, Currency.EUR, new DateTime(2021, 01, 06)));
            Assert.Equal(1.1326m, provider.GetRate(Currency.USD, Currency.EUR, new DateTime(2022, 01, 01)));
            Assert.Equal(1.2271m, provider.GetRate(Currency.USD, Currency.EUR, new DateTime(2021, 01, 01)));

            Assert.Equal(1m, provider.GetRate(Currency.EUR, Currency.EUR, new DateTime(2021, 01, 01)));
        }

        [Fact]
        public void TryGetRateTest()
        {
            var provider = EcbCurrencyProvider.Instance;

            Assert.True(provider.TryGetRate(Currency.USD, Currency.EUR, new DateTime(2021, 01, 05), out var rate));
            Assert.Equal(1.2271m, rate);
            Assert.True(provider.TryGetRate(Currency.USD, Currency.EUR, new DateTime(2019, 01, 01), out rate));
            Assert.Equal(1.145m, rate);
            Assert.True(provider.TryGetRate(Currency.EUR, Currency.EUR, new DateTime(2020, 01, 01), out rate));
            Assert.Equal(1m, rate);

            Assert.False(provider.TryGetRate(Currency.USD, Currency.EUR, new DateTime(2023, 01, 05), out rate));
            Assert.Equal(default, rate);
        }

        [Fact]
        public void ExceptionsTest()
        {
            var provider = EcbCurrencyProvider.Instance;
            var date = new DateTime(1, 1, 1);
            Assert.Throws<NotSupportedException>(() => provider.GetRate(Currency.USD, Currency.PLN, date));
            Assert.Throws<ArgumentException>(() => provider.GetRate(Currency.USD, Currency.EUR, DateTime.Now.AddDays(1)));
        }

        [Fact]
        public void CanHandleTest()
        {
            var provider = EcbCurrencyProvider.Instance;
            Assert.True(provider.CanHandle(Currency.USD, Currency.EUR));
            Assert.True(provider.CanHandle(Currency.PLN, Currency.PLN));
            Assert.False(provider.CanHandle(Currency.USD, Currency.MXN));
        }
    }
}