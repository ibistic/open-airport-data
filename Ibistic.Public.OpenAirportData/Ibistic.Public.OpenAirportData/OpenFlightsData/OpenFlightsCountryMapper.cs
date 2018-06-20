using CsvHelper.Configuration;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    internal sealed class OpenFlightsCountryMapper : ClassMap<Country>
    {
        public OpenFlightsCountryMapper()
        {
            //Skip index 0 - Airport Id
            Map(m => m.Name).ConvertUsing(row => row.GetField<string>(0).StripNullString());
            Map(m => m.Alpha2).ConvertUsing(row => row.GetField<string>(2).StripNullString());
            Map(m => m.Alpha3).Ignore();
        }

    }
}