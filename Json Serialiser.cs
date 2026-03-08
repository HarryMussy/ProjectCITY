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
                new BuildingConverter(),
                new NecessityConverter()
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
                "powerplant" => JsonSerializer.Deserialize<PowerPlant>(root.GetRawText(), options),
                "waterpump" => JsonSerializer.Deserialize<WaterPump>(root.GetRawText(), options),
                "hospital" => JsonSerializer.Deserialize<Hospital>(root.GetRawText(), options),
                "shop" => JsonSerializer.Deserialize<Shop>(root.GetRawText(), options),
                "factory" => JsonSerializer.Deserialize<Factory>(root.GetRawText(), options),
                "policebuilding" => JsonSerializer.Deserialize<PoliceBuilding>(root.GetRawText(), options),
                "fireservice" => JsonSerializer.Deserialize<FireService>(root.GetRawText(), options),


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

                case PowerPlant w:
                    JsonSerializer.Serialize(writer, w, options);
                    break;

                case WaterPump p:
                    JsonSerializer.Serialize(writer, p, options);
                    break;

                case Hospital hs:
                    JsonSerializer.Serialize(writer, hs, options);
                    break;

                case Shop s:
                    JsonSerializer.Serialize(writer, s, options);
                    break;

                case Factory f:
                    JsonSerializer.Serialize(writer, f, options);
                    break;

                case PoliceBuilding b:
                    JsonSerializer.Serialize(writer, b, options);
                    break;

                case FireService fs:
                    JsonSerializer.Serialize(writer, fs, options);
                    break;

                default:
                    throw new JsonException("Unknown Building subclass during serialization.");
            }
        }
    }

    public class NecessityConverter : JsonConverter<Necessity>
    {
        public override Necessity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            JsonElement root = doc.RootElement;

            if (!root.TryGetProperty("type", out JsonElement typeElem))
                throw new JsonException("Necessity JSON missing 'type' field.");

            string type = typeElem.GetString()?.ToLowerInvariant() ?? "";

            return type switch
            {
                "power" => JsonSerializer.Deserialize<Power>(root.GetRawText(), options),
                "water" => JsonSerializer.Deserialize<Water>(root.GetRawText(), options),
                "workers" => JsonSerializer.Deserialize<Workers>(root.GetRawText(), options),
                "ill" => JsonSerializer.Deserialize<Unhealthy>(root.GetRawText(), options),
                "crime" => JsonSerializer.Deserialize<Crime>(root.GetRawText(), options),
                "fire" => JsonSerializer.Deserialize<Fire>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown necessity type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, Necessity value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case Power p:
                    JsonSerializer.Serialize(writer, p, options);
                    break;
                case Water w:
                    JsonSerializer.Serialize(writer, w, options);
                    break;
                case Workers wk:
                    JsonSerializer.Serialize(writer, wk, options);
                    break;
                case Unhealthy u:
                    JsonSerializer.Serialize(writer, u, options);
                    break;
                case Crime c:
                    JsonSerializer.Serialize(writer, c, options);
                    break;
                case Fire f:
                    JsonSerializer.Serialize(writer, f, options);
                    break;
                default:
                    throw new JsonException("Unknown Necessity subclass during serialization.");
            }
        }
    }
}