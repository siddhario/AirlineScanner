using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirlineScanner.Core
{
  // This project can output the Class library as a NuGet Package.
  // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
  public interface IScanner
  {
    Task<IEnumerable<Flight>> GetFlights(string departureAirport, string arrivalAirport, bool roundtrip, DateTime? outboundDate, DateTime? inboundDate = null);

    Task<IEnumerable<Flight>> GetFlightsInPeriod(string departureAirport, string arrivalAirport, bool roundtrip, DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<Flight>> GetFlightMap();
  }
}
