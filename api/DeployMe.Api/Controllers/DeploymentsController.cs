using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DeployMe.Api.Models;
using DeployMe.Http;
using DeployMe.Http.WebApiExtensions;
using DeployMe.Http.WebApiExtensions.Extensions;
using DeployMe.Http.WebApiExtensions.Utility;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Abstractions;
using YamlDotNet.Serialization;

namespace DeployMe.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeploymentsController : Controller, ILogDelegate
    {
        public DeploymentsController(LogDelegate logDelegate, IRedisDatabase redisDatabase)
        {
            LogDelegate = logDelegate;
            RedisDatabase = redisDatabase;
        }

        private IRedisDatabase RedisDatabase { get; }

        public LogDelegate LogDelegate { get; }

        [HttpPost]
        public async Task<HttpActionResult<DeploymentRequest, Deployment>> Post([FromBody] DeploymentRequest request) => await this.WithResponseContainer(
            request,
            async info =>
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    throw new InternalHttpException("Deployment package name must be specified.", (int) HttpStatusCode.BadRequest, new {request});
                }

                if (string.IsNullOrEmpty(request.Version))
                {
                    throw new InternalHttpException("Deployment package version must be specified.", (int) HttpStatusCode.BadRequest, new {request});
                }

                if (request.Agents == null || request.Agents.Count == 0)
                {
                    throw new InternalHttpException("Agent list must be specified.", (int) HttpStatusCode.BadRequest, new {request});
                }

                IDeserializer deserializer = new DeserializerBuilder().Build();
                var yaml = deserializer.Deserialize<Dictionary<string, List<string>>>(new StringReader(request.Yaml));
                Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>(yaml, StringComparer.InvariantCultureIgnoreCase)
                    .ToDictionary(kv => kv.Key, kv => kv.Value ?? new List<string>());
                var deployment = new Deployment
                {
                    DeploymentPackage = new DeploymentPackage
                    {
                        Name = request.Name,
                        Version = request.Version
                    },
                    InstallCommands = dict.ContainsKey(nameof(Deployment.InstallCommands)) ? dict[nameof(Deployment.InstallCommands)] : new List<string>(),
                    StartCommands = dict.ContainsKey(nameof(Deployment.StartCommands)) ? dict[nameof(Deployment.StartCommands)] : new List<string>(),
                    StopCommands = dict.ContainsKey(nameof(Deployment.StopCommands)) ? dict[nameof(Deployment.StopCommands)] : new List<string>(),
                    UninstallCommands = dict.ContainsKey(nameof(Deployment.UninstallCommands)) ? dict[nameof(Deployment.UninstallCommands)] : new List<string>(),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Id = Guid.NewGuid().ToString()
                };

                if (deployment.InstallCommands.Count == 0 && deployment.StartCommands.Count == 0)
                {
                    throw new InternalHttpException("Must specify at least one of install/start commands.", (int) HttpStatusCode.BadRequest, new {request});
                }

                if (deployment.StopCommands.Count == 0 && deployment.UninstallCommands.Count == 0)
                {
                    throw new InternalHttpException("Must specify at least one of uninstall/stop commands.", (int) HttpStatusCode.BadRequest, new {request});
                }

                Dictionary<string, AgentInfo> agentsDict = await RedisDatabase.HashGetAllAsync<AgentInfo>(CacheKeys.AgentInfo);
                Dictionary<string, Deployment> deployments = agentsDict.Values
                    .Where(i => i.Status == AgentStatus.Ready)
                    .Where(i => request.Agents.Contains(i.Id))
                    .Select(i => i.Id)
                    .ToDictionary(k => k, v => deployment);

                await RedisDatabase.HashSetAsync(CacheKeys.ActiveDeployments, deployments);
                return deployment;
            });
    }
}
