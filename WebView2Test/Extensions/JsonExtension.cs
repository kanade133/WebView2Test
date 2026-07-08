using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebView2Test.Extensions
{
    public static class JsonExtension
    {
        public static void AddJsonSerializerOptions(this IServiceCollection services)
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            ConfigJsonOptions(jsonSerializerOptions);
            services.AddSingleton(jsonSerializerOptions);
        }
        public static void ConfigJson(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options => ConfigJsonOptions(options.SerializerOptions));
        }

        private static void ConfigJsonOptions(JsonSerializerOptions options)
        {
            options.Converters.Add(new LongToStringJsonConverter());
            options.Converters.Add(new DateTimeToTimestampJsonConverter());
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        }

        private class LongToStringJsonConverter : JsonConverter<long>
        {
            public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } stringValue)
                {
                    return long.Parse(stringValue, CultureInfo.InvariantCulture);
                }
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return 0;
                }
                return reader.GetInt64();
            }
            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private class DateTimeToTimestampJsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt64().ToUtcDateTime();
                }
                if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), CultureInfo.InvariantCulture, out var timestamp))
                {
                    return timestamp.ToUtcDateTime();
                }
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return default;
                }
                return reader.GetDateTime();
            }
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.ToTimestamp());
            }
        }
    }
}
