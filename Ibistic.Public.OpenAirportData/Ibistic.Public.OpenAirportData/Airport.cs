using System;

namespace Ibistic.Public.OpenAirportData
{
    [Serializable]
    public sealed class Airport
    {
        public string Name { get; internal set; }
        public string City { get; internal set; }
        public string CountryName { get; internal set; }
        public string CountryAlpha2 { get; internal set; }
        public string CountryAlpha3 { get; internal set; }
        public string IataCode { get; internal set; }
        public string IcaoCode { get; internal set; }
        public double Latitude { get; internal set; }
        public double Longitude { get; internal set; }
        public float AltitudeInFeet { get; internal set; }
        public float TimeZoneUtcOffset { get; internal set; }
        public DaylightSavingsTimeType DaylightSavingsTime { get; internal set; }
        public string TimeZoneName { get; internal set; }

        public override string ToString()
        {
            return $"Airport name: {Name}, City: {City}, Iata: {IataCode}, Icao: {IcaoCode}";
        }
    }
}
