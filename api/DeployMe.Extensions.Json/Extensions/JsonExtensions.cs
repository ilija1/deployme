using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DeployMe.Extensions.Json.Extensions
{
    public static class JsonExtensions
    {
        public static JObject ToJObject<T>(this T exception) where T : Exception
        {
            if (exception == null)
            {
                return null;
            }

            var serializationInfo = new SerializationInfo(typeof(T), new FormatterConverter());
            exception.GetObjectData(serializationInfo, new StreamingContext(StreamingContextStates.All));
            var dict = new Dictionary<string, object>();
            SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object value = enumerator.Current.Value;
                if (value != null && typeof(Exception).IsAssignableFrom(enumerator.Current.ObjectType))
                {
                    value = ((Exception) value).ToJObject();
                }

                dict[enumerator.Current.Name] = value;
            }

            return JObject.FromObject(dict);
        }
    }
}
