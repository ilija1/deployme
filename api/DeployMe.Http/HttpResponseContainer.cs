using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeployMe.Http
{
    public struct HttpResponseContainer<TOutput> : IHttpResponseContainer
    {
        [JsonProperty(Required = Required.AllowNull)]
        public TOutput Result { get; set; }

        [JsonProperty(Required = Required.Default)]
        public string ErrorMessage { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Code { get; set; }

        [JsonProperty(Required = Required.Default)]
        public JToken Details { get; set; }

        [JsonIgnore]
        public bool IsSuccessStatusCode => Code >= 200 && Code <= 299;

        public HttpResponseContainer<TOutput> EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
            {
                throw new InternalHttpException("Response status code does not indicate success.", Code, new {Response = this});
            }

            return this;
        }
    }
}
