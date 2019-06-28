using Microsoft.Extensions.Logging;

namespace DeployMe.Http.WebApiExtensions.Utility
{
    public delegate void LogDelegate(string message, object details = null, LogLevel logLevel = LogLevel.Information);
}
