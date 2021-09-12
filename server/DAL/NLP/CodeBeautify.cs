namespace CodeBeautify
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Welcome6
    {
        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }
    }

    public enum Tag { Adj, Adv, Ant, N, Prop, Syn, V };

    public partial class Welcome6
    {
        public static Welcome6[] FromJson(string json) => JsonConvert.DeserializeObject<Welcome6[]>(json, CodeBeautify.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome6[] self) => JsonConvert.SerializeObject(self, CodeBeautify.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TagConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class TagConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Tag) || t == typeof(Tag?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "adj":
                    return Tag.Adj;
                case "adv":
                    return Tag.Adv;
                case "ant":
                    return Tag.Ant;
                case "n":
                    return Tag.N;
                case "prop":
                    return Tag.Prop;
                case "syn":
                    return Tag.Syn;
                case "v":
                    return Tag.V;
            }
            throw new Exception("Cannot unmarshal type Tag");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Tag)untypedValue;
            switch (value)
            {
                case Tag.Adj:
                    serializer.Serialize(writer, "adj");
                    return;
                case Tag.Adv:
                    serializer.Serialize(writer, "adv");
                    return;
                case Tag.Ant:
                    serializer.Serialize(writer, "ant");
                    return;
                case Tag.N:
                    serializer.Serialize(writer, "n");
                    return;
                case Tag.Prop:
                    serializer.Serialize(writer, "prop");
                    return;
                case Tag.Syn:
                    serializer.Serialize(writer, "syn");
                    return;
                case Tag.V:
                    serializer.Serialize(writer, "v");
                    return;
            }
            throw new Exception("Cannot marshal type Tag");
        }

        public static readonly TagConverter Singleton = new TagConverter();
    }
}