# Ibistic.Public.OpenAirportData
A framework for obtaining and working with airport information from publicly available databases. The typical usecase is to resolve information such as name, country and timezone information from e.g. an IATA code.

* Data sources utilized *
Openflights Airport and Country data available from https://openflights.org/data.html under the Open Database License (https://opendatacommons.org/licenses/odbl/1.0/).

Please only use this component if you agree to and comply with the above license terms.

## Install

To install Ibistic.Public.OpenAirportData, run the following command in the Package Manager Console

    PM> Install-Package Ibistic.Public.OpenAirportData

## Documentation

Ibistic.Public.OpenAirportData has two main components: providers for downloading airport data from publicly available sources and a memory database to organize and utilize that data.

### OpenFlightsData

These providers offer access to Openflights Airport and Country data available from https://openflights.org/data.html. 

There are two providers offered; one for downloading airport data and one for downloading country data, since the first specifies countries by name and
not by ISO codes. Both providers work by downloading the data over HTTPS from OpenFlights' repository on github.com and caches it to a file specified by
the user, to avoid exessive downloads.

The typical use case for downloading airport data combined with country information is as follows:

``` csharp
using Ibistic.Public.OpenAirportData;
using Ibistic.Public.OpenAirportData.OpenFlightsData;

//...
	var airportProvider = new OpenFlightsDataAirportProvider("airports.cache", new OpenFlightsDataCountryProvider("countries.cache"));
	IEnumerable<Airport> airports = airportProvider.GetAllAirports();
```

### MemoryDatabase

If you want to keep all airports in memory and query by Iata code, you may use the class AirportIataCodeDatabase. This is used as follows:

``` csharp
using Ibistic.Public.OpenAirportData;
using Ibistic.Public.OpenAirportData.MemoryDatabase;
using Ibistic.Public.OpenAirportData.OpenFlightsData;

//...
	var airportProvider = new OpenFlightsDataAirportProvider("airports.cache", new OpenFlightsDataCountryProvider("countries.cache"));
	IEnumerable<Airport> airports = airportProvider.GetAllAirports();

	AirportIataCodeDatabase airportCodes = new AirportIataCodeDatabase();
	airportCodes.AddOrUpdateAirports(airportProvider.GetAllAirports(), true, true); //Adds airports to the database, clearing out old ones and ignoring those lacking Iata code

	string iataCode = "AGP";

	if (airportCodes.TryGetAirport(iataCode, out Airport airport)
	{
		Console.Out.WriteLine($"IATA code {iataCode} is assigned to the airport {airport.Name} in {airport.City} ({airport.CountryAlpha2})
	}
```

### Advanced use cases
Note that both the providers and memory database have code for controlling expiry. Please refer to the public properties of the different classes.

## License

This library is offered as is free for all under the MIT license: https://opensource.org/licenses/MIT

Copyright © Ibistic Technologies 2018