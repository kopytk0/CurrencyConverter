# Currency Converter

A framework to get official currency conversion rates, transaction history from brokers' APIs and automate currency conversion of transactions

## `CurrencyConverter`

- Allows converting currencies using specified provider

## Rates providers:

- `NbpCurrencyProvider`
  - Provides currency conversion rates from polish central bank to `Currency.PLN`
  - This provider is a singleton to prevent overloading API

## Transaction providers:

- `IbkrFileTransactionProvider`
  - Provides transactions from Interactive Brokers' statement in CSV format
