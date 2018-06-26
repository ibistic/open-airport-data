using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ibistic.Public.OpenAirportData.MemoryDatabase;
using Ibistic.Public.OpenAirportData.OpenFlightsData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ibistic.Public.OpenAirportData.Tests
{
    [TestClass]
    public class OpenFlightsDataProviderTests
    {
        [TestMethod]
        public void TestRawDownload()
        {
            using (var dataProvider = new OpenFlightsDataAirportProvider(Path.GetTempFileName()))
            {
                try
                {
                    byte[] data;

                    using (Stream rawDataStream = dataProvider.DownloadRawData().Result)
                    {
                        using (var memStream = new MemoryStream())
                        {
                            rawDataStream.CopyTo(memStream);
                            data = memStream.ToArray();
                        }
                    }

                    Assert.IsNotNull(data);
                    var minLength = 100000;
                    Assert.IsTrue(data.Length > minLength,
                        $"Expected raw data to be at least {minLength} bytes long. Got {data.Length} bytes");
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestCachedDownload()
        {
            string cacheFileName = Path.GetTempFileName();
            using (var dataProvider = new OpenFlightsDataAirportProvider(cacheFileName))
            {

                try
                {
                    byte[] data = GetRawData(dataProvider);

                    Assert.IsNotNull(data);
                    var minLength = 100000;
                    Assert.IsTrue(data.Length > minLength,
                        $"Expected raw data to be at least {minLength} bytes long. Got {data.Length} bytes");
                    Assert.IsTrue(File.Exists(cacheFileName),
                        $"The cache file {cacheFileName} should exist after data has been downloaded");
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }

            Assert.IsFalse(File.Exists(cacheFileName), $"The cache file {cacheFileName} should not exist after cache has been cleared");
        }

        private static byte[] GetRawData(OpenFlightsDataAirportProvider dataAirportProvider)
        {
            byte[] data;
            using (Stream rawDataStream = dataAirportProvider.GetRawData())
            {
                using (var memStream = new MemoryStream())
                {
                    rawDataStream.CopyTo(memStream);
                    data = memStream.ToArray();
                }
            }

            return data;
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException), AllowDerivedTypes = true)]
        public void TestTimeout()
        {
            string cacheFileName = Path.GetTempFileName();
            using (var dataProvider = new OpenFlightsDataAirportProvider(cacheFileName)
            {
                TimeoutValue = TimeSpan.FromTicks(1)
            })
            {
                try
                {
                    GetRawData(dataProvider);
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestCache()
        {
            string cacheFileName = Path.GetTempFileName();
            using (var dataProvider = new OpenFlightsDataAirportProvider(cacheFileName))
            {
                try
                {
                    Assert.IsTrue(dataProvider.RawDownloadCount == 0);

                    GetRawData(dataProvider);
                    Assert.IsTrue(dataProvider.RawDownloadCount == 1);

                    GetRawData(dataProvider);
                    Assert.IsTrue(dataProvider.RawDownloadCount == 1);
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestValidAirports()
        {
            using (var dataProvider = new OpenFlightsDataAirportProvider(Path.GetTempFileName()))
            {

                try
                {
                    var airports = dataProvider.GetAllAirports();
                    int totalCount = 0;
                    int missingIataCode = 0;
                    int missingCountry = 0;
                    int missingTimezoneName = 0;
                    foreach (Airport airport in airports)
                    {
                        totalCount++;
                        Assert.IsNotNull(airport);

                        if (String.IsNullOrEmpty(airport.IataCode))
                        {
                            missingIataCode++;
                        }

                        if (String.IsNullOrEmpty(airport.CountryName))
                        {
                            missingCountry++;
                        }

                        if (String.IsNullOrEmpty(airport.TimeZoneName))
                        {
                            missingTimezoneName++;
                        }
                    }

                    Console.WriteLine(
                        $"{totalCount} airports correctly read. {dataProvider.BadDataRowCount} rows skipped due to bad data.");
                    Console.WriteLine(
                        $"Missing data: iata code - {missingIataCode}, country - {missingCountry}, timezone name {missingTimezoneName}");
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestValidCountries()
        {
            using (var dataProvider = new OpenFlightsDataCountryProvider(Path.GetTempFileName()))
            {

                try
                {
                    var countries = dataProvider.GetAllCountries();
                    int totalCount = 0;
                    int missingName = 0;
                    int missingAlpha2 = 0;
                    foreach (Country country in countries)
                    {
                        totalCount++;
                        Assert.IsNotNull(country);

                        if (String.IsNullOrEmpty(country.Name))
                        {
                            missingName++;
                        }

                        if (String.IsNullOrEmpty(country.Alpha2))
                        {
                            missingAlpha2++;
                        }
                    }

                    Console.WriteLine(
                        $"{totalCount} countries correctly read. {dataProvider.BadDataRowCount} rows skipped due to bad data.");
                    Console.WriteLine(
                        $"Missing data: name - {missingName}, alpha2 - {missingAlpha2}");
                }
                finally
                {
                    dataProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestListInvalidAirports()
        {
            Dictionary<string, Country> countries;

            using (var countryProvider = new OpenFlightsDataCountryProvider(Path.GetTempFileName()))
            {
                try
                {
                    countries = countryProvider.GetAllCountries().ToDictionary(x => x.Name);
                }
                finally
                {
                    countryProvider.ClearCache();
                }
            }

            using (var airportProvider = new OpenFlightsDataAirportProvider(Path.GetTempFileName()))
            {

                try
                {
                    IEnumerable<Airport> airports = airportProvider.GetAllAirports();
                    int validAirports = 0;
                    int invalidAirports = 0;
                    int missingCountryName = 0;
                    int missingCountryMapping = 0;
                    int missingTimezoneName = 0;

                    foreach (Airport airport in airports.Where(x => !String.IsNullOrEmpty(x.IataCode)))
                    {
                        bool allValid = true;
                        if (String.IsNullOrEmpty(airport.CountryName))
                        {
                            Console.WriteLine($"Airport '{airport.Name}' ({airport.IataCode}) is missing Country name");
                            missingCountryName++;
                            allValid = false;
                        }
                        else
                        {
                            countries.TryGetValue(airport.CountryName, out Country country);

                            if (country == null)
                            {
                                Console.WriteLine($"Airport '{airport.Name}' ({airport.IataCode}) has a country '{airport.CountryName}' that cannot be mapped to Alpha 2");
                                missingCountryMapping++;
                                allValid = false;
                            }
                        }

                        if (String.IsNullOrEmpty(airport.TimeZoneName) && airport.TimeZoneUtcOffset == 0F)
                        {
                            Console.WriteLine($"Airport '{airport.Name}' ({airport.IataCode}) is missing Timezone information");
                            missingTimezoneName++;
                            allValid = false;
                        }

                        if (allValid)
                        {
                            validAirports++;
                        }
                        else
                        {
                            invalidAirports++;
                        }
                    }

                    Console.WriteLine($"Read {validAirports} valid airports. and {invalidAirports} invalid airports");
                    Console.WriteLine($"Missing info: {missingCountryName} country names, {missingCountryMapping} country mappings, {missingTimezoneName} time zones");
                }
                finally
                {
                    airportProvider.ClearCache();
                }
            }
        }

        [TestMethod]
        public void TestAirportsWithCountries()
        {
            using (var countryProvider = new OpenFlightsDataCountryProvider(Path.GetTempFileName()))
            {
                using (var airportProvider = new OpenFlightsDataAirportProvider(Path.GetTempFileName(), countryProvider))
                {
                    var airports = airportProvider.GetAllAirports();

                    var database = new AirportIataCodeDatabase();
                    database.AddOrUpdateAirports(airports, true, true);

                    Assert.IsTrue(database.GetAllAirports().Count > 1000, "Expected at least 1000 airports");

                    bool airportFound = database.TryGetAirport("AGP", out Airport malagaAirport);
                    Assert.IsTrue(airportFound);
                    Assert.IsNotNull(malagaAirport);
                    Assert.IsTrue(malagaAirport.CountryAlpha2.Equals("ES", StringComparison.Ordinal));

                    Assert.IsFalse(database.TryGetAirport("___", out _));

                }
            }
        }

    }
}
