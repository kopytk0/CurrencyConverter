## `CurrencyConverter`

Simplifies currency conversion. Uses strategy pattern for picking rate provider.
Example:
```cs
var converter = new CurrencyConverter.CurrencyConverter(new EcbRateProvider());

var zlotys = converter.Convert(Currency.EUR, Currency.PLN, DateTime(2022, 05, 25), 10m);
```

## Currency rate providers

### `EcbRateProvider`
Gets currency conversion rates from European Central Bank. 
Downloads yearly data and caches it for fast access (doesn't cache data downloaded with `GetRate(Currency, currency`

### `NbpRateProvider`
Gets currency conversion rates from Narodowy Bank Polski. 
Downloads yearly data and caches it for fast access (doesn't cache data downloaded with `GetRate(Currency, currency`

### `CoinbaseRateProvider`
Provides currency conversion rates from Coinbase.

## Broker transaction providers

### `IbkrTransactionProvider`
Gets transactions date and income from Interactive Brokers' activity statements


## `Currency` Enum
Represents currency
