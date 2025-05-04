using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using MudBlazor;
using MudblazorWeather.Backend.DataHandling;

namespace MudblazorWeather.Backend.Networking
{
    public static class HttpHandler
    {
        /// <summary>
        /// Requests most up to date data from server for current data type and appends it to the cashed data.
        /// </summary>
        public static async Task RefreshData()
        {
            DateTime now = DateTime.Now;
            //Remove seconds. The server-side averaging algorithm averages with a maximum accuracy on minutes.
            //now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + (int)Math.Round(now.Second/60.0), 0);
            
            TimeSpan requiredTimespan = now - CachedData.Instance.To;

            int neededDatapoints = (int)Math.Round(CachedData.Instance.requiredDatapointCount * (requiredTimespan / CachedData.Instance.TimeSpan));

            if (neededDatapoints == 0)
            {
                return;
            }
            
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "weatherpi.hollandweather.net",
                Path = "api/sensors.api",
                Query = $"from={CachedData.Instance.To.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Replace(' ', '_')}" +
                        $"&to={now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Replace(' ', '_')}" +
                        "&type=All" +
                        $"&data_points={neededDatapoints}"
            };
            
            CachedData.Instance.SetTimeSpan(CachedData.Instance.From + requiredTimespan, now);

            await SendHttpRequest(uriBuilder.Uri, neededDatapoints, requiredTimespan, false);
        }
        
        /// <summary>
        /// Requests a specific data type in a given time range.
        /// </summary>
        public static async Task RequestData(TimeSpan timeSpan)
        {
            CachedData.Instance.SetTimeSpan(DateTime.Now, timeSpan);
            
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "weatherpi.hollandweather.net",
                Path = "api/sensors.api",
                Query = $"from={(CachedData.Instance.From).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        $"&to={CachedData.Instance.To.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        "&type=All" +
                        $"&data_points={CachedData.Instance.requiredDatapointCount}"
            };
            
            await SendHttpRequest(uriBuilder.Uri);
        }
        
        /// <summary>
        /// Requests a specific data type in a given time range.
        /// </summary>
        public static async Task RequestData(TimeSpan timeSpan, DateTime to)
        {
            CachedData.Instance.SetTimeSpan(to, timeSpan);
            
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "weatherpi.hollandweather.net",
                Path = "api/sensors.api",
                Query = $"from={(CachedData.Instance.From).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        $"&to={CachedData.Instance.To.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        "&type=All" +
                        $"&data_points={CachedData.Instance.requiredDatapointCount}"
            };
            
            await SendHttpRequest(uriBuilder.Uri);
        }
        
        /// <summary>
        /// Requests a specific data type in a given time range.
        /// </summary>
        public static async Task RequestData(DateTime from, DateTime to)
        {
            CachedData.Instance.SetTimeSpan(from, to);
            
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "weatherpi.hollandweather.net",
                Path = "api/sensors.api",
                Query = $"from={(CachedData.Instance.From).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        $"&to={CachedData.Instance.To.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        "&type=All" +
                        $"&data_points={CachedData.Instance.requiredDatapointCount}"
            };

            await SendHttpRequest(uriBuilder.Uri);
        }
        
        /// <summary>
        /// Requests a specific data type in a given time range.
        /// </summary>
        public static async Task RequestData(DateRange range)
        {
            if (range.Start == null || range.End == null) return;
            
            CachedData.Instance.SetTimeSpan(range.Start.Value, range.End.Value + TimeSpan.FromDays(1));
            
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "weatherpi.hollandweather.net",
                Path = "api/sensors.api",
                Query = $"from={(CachedData.Instance.From).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        $"&to={CachedData.Instance.To.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture).Replace(' ', '_')}" +
                        "&type=All" +
                        $"&data_points={CachedData.Instance.requiredDatapointCount}"
            };

            await SendHttpRequest(uriBuilder.Uri);
        }
        
        private static async Task SendHttpRequest(Uri uri, int datapointCount = -1, TimeSpan range = default, bool overwriteAll = true)
        {
            Console.WriteLine($"Requesting data from Uri: \n{uri}\n");
            Stopwatch sw = Stopwatch.StartNew();
            
            using HttpClient client = new ();

            var data = await client.GetFromJsonAsync<Dictionary<DataType, Dictionary<string, List<SensorData>>>>(uri);
            
            sw.Stop();
            Console.WriteLine($"Response received after: {sw.ElapsedMilliseconds}ms.");
            sw.Start();

            if (overwriteAll)
            {
                await CachedData.Instance.SetAllData(data);
            }
            else
            {
                await CachedData.Instance.AppendData(data, range, datapointCount);
            }

            sw.Stop();
            Console.WriteLine($"Done receiving data in :{sw.ElapsedMilliseconds}ms.");
        }
    }
}