using System;
using DeployMe.Extensions.Json.Extensions;
using Newtonsoft.Json;

namespace DeployMe.Extensions.Json.Converters
{
    public sealed class ExceptionConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => typeof(Exception).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ex = (Exception) value;
            ex.ToJObject().WriteTo(writer);
        }
    }
}
