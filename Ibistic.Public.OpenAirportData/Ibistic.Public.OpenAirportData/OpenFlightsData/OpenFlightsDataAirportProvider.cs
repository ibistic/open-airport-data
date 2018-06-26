using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ibistic.Public.OpenAirportData.OpenFlightsData
{
    public sealed class OpenFlightsDataAirportProvider : OpenFlightsDataProvider
    {
        private FileStream _readerFileStream;
        private TextReader _textReader;
        private CsvReader _csvReader;

        public OpenFlightsDataAirportProvider(string cacheFileName) : base(cacheFileName)
        {
            Source = new Uri("https://raw.githubusercontent.com/jpatokal/openflights/master/data/airports.dat");
        }

        public int BadDataRowCount { get; private set; }

        public override void ClearCache()
        {
            CloseReaders();

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
            return _csvReader.GetRecords<Airport>();
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
