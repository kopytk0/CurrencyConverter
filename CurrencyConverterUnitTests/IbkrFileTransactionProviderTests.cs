﻿using System;
using System.IO;
using System.Linq;
using Jakubqwe.CurrencyConverter;
using Jakubqwe.CurrencyConverter.TransactionProviders;
using Xunit;

namespace CurrencyConverterUnitTests;

public class IbkrFileTransactionProviderTests
{
    [Fact]
    public void GetTransactionsTest()
    {
        var provider = new IbkrFileTransactionProvider();
        using var reader = File.OpenText(@"Statements\DU5421512_20220324_20220520.csv");
        var transactions = provider.GetTransactions(reader).ToList();

        Assert.Equal(4, transactions.Count);
        Assert.Equal(Currency.USD, transactions[0].BaseCurrency);
        Assert.Equal(-1.109383m, transactions[1].Income);
        Assert.Equal(new DateTime(2022, 04, 26).DayOfYear, transactions[2].Date.DayOfYear);
    }
}
