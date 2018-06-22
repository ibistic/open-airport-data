using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ibistic.Public.OpenAirportData.MemoryDatabase
{
    public class AirportIataCodeDatabase
    {
        private readonly ConcurrentDictionary<string, Airport> _airports = new ConcurrentDictionary<string, Airport>();

        public event EventHandler DatabaseExpired;

        public DateTime DataUpdated { get; private set; }
        public TimeSpan DatabaseExpiry { get; set; } = TimeSpan.FromDays(1);


        public void AddOrUpdateAirports(IEnumerable<Airport> airports, bool removeOthers = false, bool ignoreAirportsMissingIataCode = false)
        {
            var newCodes = new HashSet<string>();

            foreach (Airport airport in airports)
            {
                var iataCode = airport.IataCode;

                if (String.IsNullOrEmpty(iataCode))
                {
                    if (ignoreAirportsMissingIataCode)
                    {
                        continue;;
                    }

                    throw new ArgumentException($"The airport {airport} is missing iata code. Filter the data prior to call this method, or call it with {nameof(ignoreAirportsMissingIataCode)} set to true.");
                }

                _airports[iataCode] = airport;
                newCodes.Add(iataCode);
            }

            if (removeOthers)
            {
                var airportsToRemove = _airports.Keys.Except(newCodes);

                foreach (string iataCodeToRemove in airportsToRemove)
                {
                    _airports.TryRemove(iataCodeToRemove, out _);
                }
            }

            DataUpdated = DateTime.UtcNow;
        }

        public IReadOnlyCollection<Airport> GetAllAirports()
        {
            NotifyIfExpired();

            return _airports.Values.ToArray();
        }

        public bool TryGetAirport(string iataCode, out Airport airport)
        {
            NotifyIfExpired();

            return _airports.TryGetValue(iataCode, out airport);
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
