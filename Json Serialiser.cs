using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CitySkylines0._5alphabeta
{
    public static class JsonSettings
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters =
            {
                new JsonStringEnumConverter(),
                new BuildingConverter()     // <-- REGISTERED HERE
            }
        };
    }

    public class BuildingConverter : JsonConverter<Building>
    {
        public override Building Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse the incoming JSON object
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("type", out JsonElement typeElem))
                throw new JsonException("Building JSON missing 'type' field.");

            string type = typeElem.GetString()?.ToLowerInvariant();

            return type switch
            {
                "house" => JsonSerializer.Deserialize<House>(root.GetRawText(), options),
                "windfarm" => JsonSerializer.Deserialize<Windfarm>(root.GetRawText(), options),
                "waterpump" => JsonSerializer.Deserialize<WaterPump>(root.GetRawText(), options),

                _ => throw new JsonException($"Unknown building type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Building value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case House h:
                    JsonSerializer.Serialize(writer, h, options);
                    break;

                case Windfarm w:
                    JsonSerializer.Serialize(writer, w, options);
                    break;

                case WaterPump p:
                    JsonSerializer.Serialize(writer, p, options);
                    break;

                default:
                    throw new JsonException("Unknown Building subclass during serialization.");
            }
        }
    }
}
