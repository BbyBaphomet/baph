
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace BaphsFika.Plugin.Utils
{
    public static class JsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                new Vector3Converter(),
                new QuaternionConverter()
            }
        };

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static string SerializeCompressed<T>(T obj)
        {
            string json = Serialize(obj);
            return Compression.Compress(json);
        }

        public static T DeserializeCompressed<T>(string compressedJson)
        {
            string json = Compression.Decompress(compressedJson);
            return Deserialize<T>(json);
        }
    }

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var vector = new Vector3();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    reader.Read();
                    switch (propertyName.ToLower())
                    {
                        case "x":
                            vector.x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            vector.y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            vector.z = Convert.ToSingle(reader.Value);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return vector;
                }
            }
            throw new JsonSerializationException("Error parsing Vector3");
        }
    }

    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var quaternion = new Quaternion();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = reader.Value.ToString();
                    reader.Read();
                    switch (propertyName.ToLower())
                    {
                        case "x":
                            quaternion.x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            quaternion.y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            quaternion.z = Convert.ToSingle(reader.Value);
                            break;
                        case "w":
                            quaternion.w = Convert.ToSingle(reader.Value);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    return quaternion;
                }
            }
            throw new JsonSerializationException("Error parsing Quaternion");
        }
    }
}
