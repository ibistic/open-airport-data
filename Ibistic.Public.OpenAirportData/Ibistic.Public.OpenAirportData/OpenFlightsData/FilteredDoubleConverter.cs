using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    internal sealed class FilteredDoubleConverter : DoubleConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.IsNullStringOrEmpty())
            {
                return default(float);
            }

            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}