using AirlineScanner.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AirlineScanner.Test
{
  // This project can output the Class library as a NuGet Package.
  // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
  public class VuelingScannerTest
  {
    public VuelingScannerTest()
    {
    }

    //[Fact]
    public async void GetFlightsTest()
    {
      IScanner s = new VuelingScanner();
      var flights = await s.GetFlights("BEG", "BCN", true, DateTime.Parse("2016-07-05"), DateTime.Parse("2016-07-05"));
      foreach (var flight in flights)
        Console.WriteLine(flight.DepartureTime + ":" + flight.DepartureAirportCode + "-" + flight.ArrivalAirportCode + " " + flight.Price);

      flights = await s.GetFlights("ZAG", "BCN", true, DateTime.Parse("2016-07-06"), DateTime.Parse("2016-07-06"));
      foreach (var flight in flights)
        Console.WriteLine(flight.DepartureTime + ":" + flight.DepartureAirportCode + "-" + flight.ArrivalAirportCode + " " + flight.Price);
      Console.ReadKey();
    }

    [Fact]
    public async void GetFlightMapTest()
    {
      IScanner s = new VuelingScanner();
      var flightmap = await s.GetFlightMap();
      foreach (var flight in flightmap)
        Console.WriteLine(flight.DepartureAirportCode + "-" + flight.ArrivalAirportCode);
      Console.ReadKey();
    }

    //[Fact]
    public async void GetFlightsInPeriodTest()
    {
      IScanner s = new VuelingScanner();
      var flights = await s.GetFlightsInPeriod("ALC", "BEG", true, DateTime.Parse("2016-07-01"), DateTime.Parse("2016-07-05"));
      foreach (var flight in flights)
        Console.WriteLine(flight.DepartureTime + "-" + flight.ArrivalTime + ":" + flight.DepartureAirportCode + "-" + flight.ArrivalAirportCode + " " + flight.Price);
      Console.ReadKey();
    }
  }
}
