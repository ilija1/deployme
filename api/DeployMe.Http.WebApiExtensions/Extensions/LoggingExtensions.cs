using System;
using DeployMe.Http.WebApiExtensions.Utility;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeployMe.Http.WebApiExtensions.Extensions
{
    public static class LoggingExtensions
    {
        public static void LogJson(this ILogger logger, string component, string controller, string action, LogLevel logLevel, string message, object details = null)
        {
            logger?.Log(
                logLevel,
                default(EventId),
                (object) null,
                null,
                (o, ex) =>
                {
                    DateTimeOffset now = DateTimeOffset.UtcNow;
                    JObject jObj;
                    if (ex == null)
                    {
                        jObj = JObject.FromObject(
                            new
                            {
                                TimestampISO = now.ToString("O"),
                                Timestamp = now.ToUnixTimeMilliseconds(),
                                Component = component,
                                Controller = controller,
                                Action = action,
                                LogLevel = logLevel,
                                Message = message,
                                Details = details
                            });
                    }
                    else
                    {
                        jObj = JObject.FromObject(
                            new
                            {
                                TimestampISO = now.ToString("O"),
                                Timestamp = now.ToUnixTimeMilliseconds(),
                                Component = component,
                                Controller = controller,
                                Action = action,
                                LogLevel = logLevel,
                                Message = message,
                                Details = details,
                                Exception = ex
                            });
                    }

                    string json = jObj.ToString(Formatting.None);
                    return json;
                });
        }

        public static IServiceCollection RegisterLogDelegate(this IServiceCollection services, string componentName)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton(i => i.GetService<ILoggerFactory>()?.CreateLogger(componentName));
            services.AddSingleton<LogDelegate>(
                i =>
                {
                    var logger = i.GetService<ILogger>();
                    return (message, details, level) =>
                    {
                        var descriptor = (ControllerActionDescriptor) i.GetRequiredService<IActionContextAccessor>().ActionContext.ActionDescriptor;
                        logger?.LogJson(
                            componentName,
                            descriptor.ControllerTypeInfo.FullName,
                            descriptor.ActionName,
                            level,
                            message,
                            details);
                    };
                });
            return services;
        }
    }
}
