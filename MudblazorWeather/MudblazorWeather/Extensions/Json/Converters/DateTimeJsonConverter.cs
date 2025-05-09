using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MudblazorWeather.Extensions.Json.Converters
{
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            Convert.ToDateTime(reader.GetString());

            

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(dateTimeValue.ToString(
                "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
    }
}