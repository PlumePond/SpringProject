using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpringProject.Core.SaveSystem;

public class StringifyValuesConverter : JsonConverter<Dictionary<string, string>>
{
    public override Dictionary<string, string> ReadJson(JsonReader reader, Type objectType, Dictionary<string, string> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new Dictionary<string, string>();
        var raw = serializer.Deserialize<Dictionary<string, object>>(reader);
        if (raw == null) return result;

        foreach (var kvp in raw)
        {
            result[kvp.Key] = kvp.Value is Newtonsoft.Json.Linq.JToken token
                ? token.ToString(Formatting.None)
                : JsonConvert.SerializeObject(kvp.Value);
        }

        return result;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, string> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            try
            {
                // Write the value as a raw JSON token, not a quoted string
                JToken.Parse(kvp.Value).WriteTo(writer);
            }
            catch (JsonException)
            {
                // Not valid JSON (plain string like "hello") — write as a string
                writer.WriteValue(kvp.Value);
            }
        }
        writer.WriteEndObject();
    }
}