using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    public sealed class OpenFlightsDataAirportProvider : OpenFlightsDataProvider
    {
        private readonly OpenFlightsDataCountryProvider _countryProvider;
        private FileStream _readerFileStream;
        private TextReader _textReader;
        private CsvReader _csvReader;

        public OpenFlightsDataAirportProvider(string cacheFileName, OpenFlightsDataCountryProvider countryProvider = null) : base(cacheFileName)
        {
            _countryProvider = countryProvider;
            Source = new Uri("https://raw.githubusercontent.com/jpatokal/openflights/master/data/airports.dat");
        }

        public int BadDataRowCount { get; private set; }

        public override void ClearCache()
        {
            CloseReaders();
            _countryProvider?.ClearCache();

            base.ClearCache();
        }

        public IEnumerable<Airport> GetAllAirports()
        {
            EnsureNotDisposed();
            CloseReaders();

            _readerFileStream = GetRawData();
            _textReader = new StreamReader(_readerFileStream, SourceEncoding);
            var configuration = new Configuration
            {
                AllowComments = true,
                DetectColumnCountChanges = true,
                HasHeaderRecord = false,
                Delimiter = ",",
                CultureInfo = CultureInfo.InvariantCulture
            };
            configuration.RegisterClassMap<OpenFlightsAirportMapper>();
            configuration.BadDataFound = context => BadDataRowCount++;

            _csvReader = new CsvReader(_textReader, configuration);
            var airports =  _csvReader.GetRecords<Airport>();

            return _countryProvider == null ? airports : AddCountryInformation(airports);
        }

        private IEnumerable<Airport> AddCountryInformation(IEnumerable<Airport> airports)
        {
            Dictionary<string, Country> countriesByName = _countryProvider.GetAllCountries().Where(x => !String.IsNullOrEmpty(x.Name))
                .ToDictionary(x => x.Name);

            foreach (var airport in airports)
            {
                if (String.IsNullOrEmpty(airport.CountryAlpha2) && String.IsNullOrEmpty(airport.CountryAlpha3) && !String.IsNullOrEmpty(airport.CountryName))
                {
                    if (countriesByName.TryGetValue(airport.CountryName, out Country country))
                    {
                        airport.CountryAlpha2 = country.Alpha2;
                        airport.CountryAlpha3 = country.Alpha3;
                    }
                }

                yield return airport;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                CloseReaders();
            }

            base.Dispose(disposing);
        }

        private void CloseReaders()
        {
            _csvReader?.Dispose();
            _csvReader = null;

            _textReader?.Dispose();
            _textReader = null;

            _readerFileStream?.Dispose();
            _readerFileStream = null;
        }
    }
}
