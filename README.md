[![NuGet](https://img.shields.io/nuget/v/Jakubqwe.CurrencyConverter)](https://www.nuget.org/packages/Jakubqwe.CurrencyConverter/)
# Currency Converter

A library for currency conversion

## `CurrencyConverter`

- Allows converting currencies using specified provider

## Rates providers:

- `NbpCurrencyProvider`
  - Provides currency conversion rates from Polish central bank
  - Downloads yearly data and caches it (doesn't cache requests for newest rates)
  - Source: http://api.nbp.pl/
- `EcbCurrencyProvider`
  - Provides currency conversion rates from European central bank
  - Downloads yearly data and caches it (doesn't cache requests for newest rates)
  - Has asynchronous methods
  - Source: https://sdw-wsrest.ecb.europa.eu/
- `CoinbaseCurrencyProvider`
  - Provides currency conversion rates from Coinbase
  - Only supports latest rates
  - Source: https://docs.cloud.coinbase.com/sign-in-with-coinbase/docs/api-exchange-rates
## Transaction providers:

- `IbkrFileTransactionProvider`
  - Provides transactions from Interactive Brokers' statement in CSV format
