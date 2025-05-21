using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BannanagramsLibrary
{
    public class ObjectWithTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(object);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var token = JToken.FromObject(value, serializer);
            token.WriteTo(writer);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.ToObject<object>(serializer);
        }
    }
}