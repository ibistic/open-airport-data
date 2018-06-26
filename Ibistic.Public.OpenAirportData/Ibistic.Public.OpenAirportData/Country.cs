using System;

namespace Ibistic.Public.OpenAirportData
{
    [Serializable]
    public sealed class Country
    {
        public string Name { get; internal set; }
        public string Alpha2 { get; internal set; }
        public string Alpha3 { get; internal set; }

        public override string ToString()
        {
            return $"Country name: {Name}, Alpha2: {Alpha2}, Alpha3: {Alpha3}";
        }
    }
}
