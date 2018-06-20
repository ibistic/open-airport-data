using CsvHelper.Configuration;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    internal sealed class OpenFlightsAirportMapper : ClassMap<Airport>
    {
        public OpenFlightsAirportMapper()
        {
            var singleConverter = new FilteredSingleConverter();
            var doubleConverter = new FilteredDoubleConverter();

            //Skip index 0 - Airport Id
            Map(m => m.Name).ConvertUsing(row => row.GetField<string>(1).StripNullString());
            Map(m => m.City).ConvertUsing(row => row.GetField<string>(2).StripNullString());
            Map(m => m.CountryName).ConvertUsing(row => row.GetField<string>(3).StripNullString());
            Map(m => m.CountryAlhpa2).Ignore();
            Map(m => m.CountryAlpha3).Ignore();
            Map(m => m.IataCode).ConvertUsing(row => row.GetField<string>(4).StripNullString());
            Map(m => m.IcaoCode).ConvertUsing(row => row.GetField<string>(5).StripNullString());
            Map(m => m.Latitude).ConvertUsing(row => row.GetField<double>(6, doubleConverter));
            Map(m => m.Longitude).ConvertUsing(row => row.GetField<double>(7, doubleConverter));
            Map(m => m.AltitudeInFeet).ConvertUsing(row => row.GetField<float>(8, singleConverter));
            Map(m => m.TimezoneUtcOffset).ConvertUsing(row => row.GetField<float>(9, singleConverter));
            Map(m => m.DaylightSavingsTime).ConvertUsing(
                row =>
                {
                    var field = row.GetField(10).StripNullString();

                    if (field == null)
                    {
                        return DaylightSavingsTimeType.Unknown;
                    }

                    switch (field.ToUpperInvariant())
                    {
                        case "N":
                            return DaylightSavingsTimeType.None;
                        case "E":
                            return DaylightSavingsTimeType.Europe;
                        case "A":
                            return DaylightSavingsTimeType.UsCanada;
                        case "S":
                            return DaylightSavingsTimeType.SouthAmerica;
                        case "O":
                            return DaylightSavingsTimeType.Australia;
                        case "Z":
                            return DaylightSavingsTimeType.NewZeland;
                        // ReSharper disable once RedundantCaseLabel
                        case "U":
                        default:
                            return DaylightSavingsTimeType.Unknown;
                    }
                }
            );
            Map(m => m.TimezoneName).ConvertUsing(row => row.GetField<string>(11).StripNullString());
        }
    }
}