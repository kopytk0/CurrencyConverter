using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokerConverter
{
    internal class YearlyCurrencyRates
    {
        private readonly Dictionary<ushort, decimal[]> _exchangeRatesInYear = new();
        public Currency TargetCurrency { get; }
        public Currency SourceCurrency { get; }

        internal YearlyCurrencyRates(Currency sourceCurrency, Currency targetCurrency)
        {
            if (targetCurrency == sourceCurrency)
            {
                throw new ArgumentException("Target currency must be different than source currency");
            }

            TargetCurrency = targetCurrency;
            SourceCurrency = sourceCurrency;
        }

        /// <summary>
        /// Adds exchange rates for given year
        /// </summary>
        internal YearlyCurrencyRates(Currency sourceCurrency, Currency targetCurrency, ushort year, IEnumerable<KeyValuePair<DateTime, decimal>> rates) 
            : this(sourceCurrency, targetCurrency)
        {
            SetRates(year, rates);
        }

        public decimal GetRate(DateTime date)
        {
            if (!_exchangeRatesInYear.TryGetValue((ushort)date.Year, out var rates))
            {
                throw new KeyNotFoundException("No exchange rates for year " + date.Year);
            }

            var rate = rates[date.DayOfYear - 1];
            return rate != default ? rate : throw new KeyNotFoundException("No exchange rate for day " + date.DayOfYear);
        }

        public bool TryGetRate(DateTime date, out decimal rate)
        {
            if (!_exchangeRatesInYear.TryGetValue((ushort)date.Year, out decimal[] rates))
            {
                rate = default;
                return false;
            }

            rate = rates[date.DayOfYear - 1];
            return rate != default;
        }

        public bool ContainsRate(DateTime date)
        {
            if (!_exchangeRatesInYear.TryGetValue((ushort)date.Year, out decimal[] rates))
            {
                return false;
            }

            return rates[date.DayOfYear - 1] != default;
        }

        public bool ContainsYear(ushort year)
        {
            return _exchangeRatesInYear.ContainsKey(year);
        }

        public void SetRates(ushort year, IEnumerable<KeyValuePair<DateTime, decimal>> rates)
        {
            var array = new decimal[year % 4 == 0 ? 366 : 365];
            foreach (var rate in rates)
            {
                array[rate.Key.DayOfYear - 1] = rate.Value;
            }

            var lastKnownRate = default(decimal);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == default)
                {
                    array[i] = lastKnownRate;
                    continue;
                }

                lastKnownRate = array[i];
            }
            _exchangeRatesInYear.Add(year, array);

            FillFirstDaysOfYear(year);
            FillFirstDaysOfYear((ushort)(year + 1), lastKnownRate);

        }

        private void FillFirstDaysOfYear(ushort year, decimal value = default)
        {
            if (value == default && !TryGetRate(new DateTime(year - 1, 12, 31), out value))
            {
                return;
            }
            
            if (!_exchangeRatesInYear.TryGetValue(year, out var rates))
            {
                return;
            }

            for (int i = 0; i < rates.Length; i++)
            {
                if (rates[i] != default)
                {
                    break;
                }

                rates[i] = value;
            }
        }
    }
}