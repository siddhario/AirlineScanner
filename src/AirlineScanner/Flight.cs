using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirlineScanner
{
  public class Flight
  {
    public string DepartureAirportCode { get; set; }
    public string ArrivalAirportCode { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public decimal Price { get; set; }

  }
}
