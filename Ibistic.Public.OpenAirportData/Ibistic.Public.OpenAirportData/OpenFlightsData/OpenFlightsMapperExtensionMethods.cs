using System;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    internal static class OpenFlightsMapperExtensionMethods
    {
        public const string NullString = "\\N";
        public static string StripNullString(this string input)
        {
            return input == null ? null : (input.Equals(NullString, StringComparison.OrdinalIgnoreCase) ? null : input);
        }

        public static bool IsNullStringOrEmpty(this string input)
        {
            return String.IsNullOrEmpty(input.StripNullString());
        }
    }
}