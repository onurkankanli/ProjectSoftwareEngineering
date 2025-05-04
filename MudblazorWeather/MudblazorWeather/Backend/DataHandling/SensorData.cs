using System;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MudblazorWeather.Extensions.Json.Converters;

namespace MudblazorWeather.Backend.DataHandling
{
    public class SensorData
    {
        public SensorData(DateTime timestamp, double value)
        {
            this.timestamp = timestamp;
            this.value = value;
        }
        
        [JsonInclude]
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime timestamp;
        
        [JsonInclude]
        public double value;
        
        
        public static implicit operator double(SensorData data) => data.value;
    }
}