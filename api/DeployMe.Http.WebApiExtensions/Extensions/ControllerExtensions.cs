using System;
using System.Net;
using System.Threading.Tasks;
using DeployMe.Http.WebApiExtensions.Utility;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DeployMe.Http.WebApiExtensions.Extensions
{
    public static class ControllerExtensions
    {
        public static JsonResult GetExceptionContainer<TException, TController>(this TController controller, TException ex)
            where TException : Exception
            where TController : Controller, ILogDelegate
        {
            string requestId = Guid.NewGuid().ToString();

            var details = new
            {
                ex,
                requestId,
                uri = controller.Request?.GetDisplayUrl(),
                headers = controller.Request?.Headers,
                ipAddress = controller.Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            controller.LogDelegate?.Invoke(ex.Message, details, LogLevel.Error);

            int code;
            JToken detailsObj;

            var internalHttpException = ex as InternalHttpException;
            if (internalHttpException != null)
            {
                code = internalHttpException.Code;
                detailsObj = internalHttpException.Details;
            }
            else
            {
                code = (int) HttpStatusCode.InternalServerError;
                detailsObj = JToken.FromObject(new {ex});
            }

            return new JsonResult(new HttpResponseContainer<object> {Code = code, ErrorMessage = ex.Message, Details = detailsObj}) {StatusCode = code};
        }

        public static async Task<JsonResult> GetResponseContainer<TOutput, TController>(this TController controller, Func<Task<TOutput>> action)
            where TController : Controller, ILogDelegate
        {
            try
            {
                TOutput result = await action();

                if (result is IHttpResponseContainer containerResponse)
                {
                    return new HttpActionResult<TOutput>(result) {StatusCode = containerResponse.Code};
                }

                return new HttpActionResult<TOutput>(new HttpResponseContainer<TOutput> {Code = (int) HttpStatusCode.OK, Result = result}) {StatusCode = (int) HttpStatusCode.OK};
            }
            catch (InternalHttpException ex)
            {
                return controller.GetExceptionContainer(ex);
            }
            catch (Exception ex)
            {
                return controller.GetExceptionContainer(ex);
            }
        }

        public static async Task<HttpActionResult<TInput, TOutput>> WithResponseContainer<TInput, TOutput, TController>(
            this TController controller,
            TInput input,
            Func<TInput, Task<TOutput>> action)
            where TController : Controller, ILogDelegate
            => new HttpActionResult<TInput, TOutput>(await controller.GetResponseContainer(async () => await action(input)));

        public static async Task<HttpActionResult<TOutput>> WithResponseContainer<TOutput, TController>(this TController controller, Func<Task<TOutput>> action)
            where TController : Controller, ILogDelegate
            => new HttpActionResult<TOutput>(await controller.GetResponseContainer(action));
    }
}
