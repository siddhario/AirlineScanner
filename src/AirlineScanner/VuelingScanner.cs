using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirlineScanner.Core
{
  public class VuelingScanner : IVuelingScanner
  {
    public async Task<IEnumerable<Flight>> GetFlightMap()
    {
      string url = "http://www.vueling.com/en/book-your-flight/where-we-fly";
      StreamReader sr = new StreamReader(@"C:\Users\djekicd\Desktop\flightmap.txt");
      bool online = false;
      var flights = new List<Flight>();
      try
      {
        HttpResponseMessage response = null;
        do
        {
          try
          {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0");
            response = await httpClient.SendAsync(requestMessage);
            online = true;
          }
          catch (Exception e)
          {
            online = false;
            await Task.Delay(5000);
          }
        }
        while (online == false);

        HtmlDocument htmlDocument = new HtmlDocument();
        string ss = await response.Content.ReadAsStringAsync();

        // string ss = sr.ReadToEnd();

        string[] splits = Regex.Split(ss, "conForDest");
        for (int i = 1; i < splits.Length; i++)
        {
          var split = splits[i];
          var json = split.Substring(2, split.IndexOf("]") - 1);
          JArray obj = Newtonsoft.Json.JsonConvert.DeserializeObject(json) as JArray;
          foreach (dynamic o in obj)
          {
            string orig = o.orig;
            string con = o.con;
            string dest = o.dest;
            string carr = o.carr;
            if (con != null && con != "")
              dest = con;
            if (flights.Where(f => f.DepartureAirportCode == orig && f.ArrivalAirportCode == dest).FirstOrDefault() == null)
              flights.Add(new Flight() { DepartureAirportCode = orig, ArrivalAirportCode = dest });
          }

        }
      }
      catch (Exception e)
      {
        //log
      }
      return flights.OrderBy(f => f.DepartureAirportCode);
    }

    public async Task<IEnumerable<Flight>> GetFlights(string departureAirport, string arrivalAirport, bool roundtrip, DateTime? outboundDate, DateTime? inboundDate)
    {
      bool online = false;
      var flights = new List<Flight>();
      string url = @"http://tickets.vueling.com/ScheduleSelect.aspx?__EVENTTARGET=AvailabilitySearchInputSearchView$LinkButtonNewSearch&AvailabilitySearchInputSearchView$DropDownListSearchBy=columnView&AvailabilitySearchInputSearchView$RadioButtonMarketStructure=RoundTrip&departureStationCode1="
+ departureAirport
+ @"&arrivalStationCode1=" + arrivalAirport
+ @"&AvailabilitySearchInputSearchView$DropDownListMarketDay1=" + outboundDate.Value.Day.ToString("D2")
      + @"&AvailabilitySearchInputSearchView$DropDownListMarketMonth1=" + outboundDate.Value.Year + "-" + outboundDate.Value.Month.ToString("D2")
      + @"&Culture=en-GB&PromoAbTesting=undefined&AvailabilitySearchInputSearchView$DropDownListMarketDay2=" + inboundDate.Value.Day.ToString("D2")
      + @"&AvailabilitySearchInputSearchView$DropDownListMarketMonth2=" + inboundDate.Value.Year + "-" + inboundDate.Value.Month.ToString("D2")
      + @"&AvailabilitySearchInputSearchView$DropDownListPassengerType_ADT=1&AvailabilitySearchInputSearchView$DropDownListPassengerType_CHD=0&AvailabilitySearchInputSearchView$DropDownListPassengerType_INFANT=0";

      // url = "http://tickets.vueling.com/ScheduleSelect.aspx?__EVENTTARGET=AvailabilitySearchInputSearchView$LinkButtonNewSearch&AvailabilitySearchInputSearchView$DropDownListSearchBy=columnView&AvailabilitySearchInputSearchView$RadioButtonMarketStructure=RoundTrip&departureStationCode1=BCN&arrivalStationCode1=AMS&AvailabilitySearchInputSearchView$DropDownListMarketDay1=27&AvailabilitySearchInputSearchView$DropDownListMarketMonth1=2016-02&Culture=en-GB&PromoAbTesting=undefined&AvailabilitySearchInputSearchView$DropDownListMarketDay2=29&AvailabilitySearchInputSearchView$DropDownListMarketMonth2=2016-02&AvailabilitySearchInputSearchView$DropDownListPassengerType_ADT=1&AvailabilitySearchInputSearchView$DropDownListPassengerType_CHD=0&AvailabilitySearchInputSearchView$DropDownListPassengerType_INFANT=0";
      try
      {
        HttpResponseMessage response = null;
        do
        {
          try
          {
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0");
            response = await httpClient.SendAsync(requestMessage);
            online = true;
          }
          catch (Exception e)
          {
            online = false;
            await Task.Delay(5000);
          }
        }
        while (online == false);

        HtmlDocument htmlDocument = new HtmlDocument();
        string ss = await response.Content.ReadAsStringAsync();
        var data = ss.Substring(ss.IndexOf("\"markets\":["));
        data = "{" + data.Substring(0, data.IndexOf("]});") + 2);
        dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

        foreach (dynamic journey in obj.markets[0].journeys)
        {
          string departureTime = journey.departureTime;
          string arrivalTime = journey.arrivalTime;
          string basicPriceRoute = journey.basicPriceRoute;

          flights.Add(new Flight() { DepartureAirportCode = departureAirport, ArrivalAirportCode = arrivalAirport, DepartureTime = DateTime.Parse(departureTime), ArrivalTime = DateTime.Parse(arrivalTime), Price = decimal.Parse(basicPriceRoute) });
        }

        foreach (dynamic journey in obj.markets[1].journeys)
        {
          string departureTime = journey.departureTime;
          string arrivalTime = journey.arrivalTime;
          string basicPriceRoute = journey.basicPriceRoute;

          flights.Add(new Flight() { DepartureAirportCode = arrivalAirport, ArrivalAirportCode = departureAirport, DepartureTime = DateTime.Parse(departureTime), ArrivalTime = DateTime.Parse(arrivalTime), Price = decimal.Parse(basicPriceRoute) });
        }

      }
      catch (Exception e)
      {
        //log
      }
      return flights;
    }

    public async Task<IEnumerable<Flight>> GetFlightsInPeriod(string departureAirport, string arrivalAirport, bool roundtrip, DateTime? startDate, DateTime? endDate)
    {
      List<Flight> flights = new List<Flight>();
      int daySpan = 0;
      while (startDate.Value.AddDays(daySpan) <= endDate)
      {
        Console.WriteLine(startDate.Value.AddDays(daySpan));
        flights.InsertRange(0, await GetFlights(departureAirport, arrivalAirport, roundtrip, startDate.Value.AddDays(daySpan), startDate.Value.AddDays(daySpan)));
        daySpan++;
        await Task.Delay(300);
      }
      return flights.OrderByDescending(f => f.DepartureTime);
    }
  }
}