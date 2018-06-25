using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ibistic.Public.OpenAirportData.MemoryDatabase
{
    public class AirportIataCodeDatabase
    {
        private readonly ConcurrentDictionary<string, Airport> _airportsByIataCode = new ConcurrentDictionary<string, Airport>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler DatabaseExpired;

        public DateTime DataUpdated { get; private set; }
        public TimeSpan DatabaseExpiry { get; set; } = TimeSpan.FromDays(1);


        public void AddOrUpdateAirports(IEnumerable<Airport> airports, bool removeOthers = false, bool ignoreAirportsMissingIataCode = false)
        {
            if (airports == null)
            {
                throw new ArgumentNullException(nameof(airports));
            }

            var newCodes = new HashSet<string>();

            foreach (Airport airport in airports)
            {
                var iataCode = airport.IataCode;

                if (String.IsNullOrEmpty(iataCode))
                {
                    if (ignoreAirportsMissingIataCode)
                    {
                        continue;
                    }

                    throw new ArgumentException($"The airport {airport} is missing iata code. Filter the data prior to call this method, or call it with {nameof(ignoreAirportsMissingIataCode)} set to true.");
                }

                _airportsByIataCode[iataCode] = airport;
                newCodes.Add(iataCode);
            }

            if (newCodes.Count == 0) //Late validation of argument to avoid multiple enumeration
            {
                throw new ArgumentException($"At least one airport required. To empty, use the method {nameof(Clear)}.");
            }

            if (removeOthers)
            {
                var airportsToRemove = _airportsByIataCode.Keys.Except(newCodes);

                foreach (string iataCodeToRemove in airportsToRemove)
                {
                    _airportsByIataCode.TryRemove(iataCodeToRemove, out _);
                }
            }

            DataUpdated = DateTime.UtcNow;
        }

        public void Clear()
        {
            _airportsByIataCode.Clear();
            DataUpdated = default(DateTime);
        }

        public IReadOnlyCollection<Airport> GetAllAirports()
        {
            NotifyIfExpired();

            return _airportsByIataCode.Values.ToArray();
        }

        public bool TryGetAirport(string iataCode, out Airport airport)
        {
            if (String.IsNullOrEmpty(iataCode))
            {
                throw new ArgumentException($"Iata code must be provided", nameof(iataCode));
            }

            NotifyIfExpired();

            return _airportsByIataCode.TryGetValue(iataCode, out airport);
        }

        private void NotifyIfExpired()
        {
            if (DataUpdated.Add(DatabaseExpiry) > DateTime.UtcNow)
            {
                return;
            }

            DatabaseExpired?.Invoke(this, EventArgs.Empty);
        }
    }
}
