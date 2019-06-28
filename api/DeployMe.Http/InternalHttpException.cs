using System;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DeployMe.Http
{
    public class InternalHttpException : Exception
    {
        public InternalHttpException(string message, int code = (int) HttpStatusCode.InternalServerError, object details = null)
            : base(message)
        {
            Code = code;
            Details = details == null ? null : JToken.FromObject(details);
        }

        public int Code { get; set; }

        public JToken Details { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Code), Code);
            info.AddValue(nameof(Details), Details);
        }
    }
}
