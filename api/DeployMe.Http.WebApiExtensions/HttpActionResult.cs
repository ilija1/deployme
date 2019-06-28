using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeployMe.Http.WebApiExtensions
{
    public class HttpActionResult<TOutput> : JsonResult
    {
        public HttpActionResult(object value)
            : base(value)
        {
        }

        public HttpActionResult(object value, JsonSerializerSettings serializerSettings)
            : base(value, serializerSettings)
        {
        }

        public HttpActionResult(JsonResult value)
            : base(value.Value)
        {
            StatusCode = value.StatusCode;
            if (value.SerializerSettings != null)
            {
                SerializerSettings = value.SerializerSettings;
            }
        }
    }

    public class HttpActionResult<TInput, TOutput> : HttpActionResult<TOutput>
    {
        public HttpActionResult(object value)
            : base(value)
        {
        }

        public HttpActionResult(object value, JsonSerializerSettings serializerSettings)
            : base(value, serializerSettings)
        {
        }

        public HttpActionResult(JsonResult value)
            : base(value.Value)
        {
            StatusCode = value.StatusCode;
            if (value.SerializerSettings != null)
            {
                SerializerSettings = value.SerializerSettings;
            }
        }
    }
}
