using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeployMe.Api.Models;
using DeployMe.Http.WebApiExtensions;
using DeployMe.Http.WebApiExtensions.Extensions;
using DeployMe.Http.WebApiExtensions.Utility;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace DeployMe.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CommunicationController : Controller, ILogDelegate
    {
        public CommunicationController(LogDelegate logDelegate, IRedisDatabase redisDatabase)
        {
            LogDelegate = logDelegate;
            RedisDatabase = redisDatabase;
        }

        private IRedisDatabase RedisDatabase { get; }

        public LogDelegate LogDelegate { get; }

        [HttpPost("poll")]
        public async Task<HttpActionResult<AgentInfo, DeploymentCommand>> Poll([FromBody] AgentInfo agentInfo) => await this.WithResponseContainer(
            agentInfo,
            async info =>
            {
                var command = new DeploymentCommand
                {
                    Execute = true
                };

                string sourceIp = Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var newInfo = new AgentInfo
                {
                    DeploymentPackage = info.DeploymentPackage,
                    ActiveDeploymentId = info.ActiveDeploymentId,
                    ReportedIpAddresses = info.ReportedIpAddresses,
                    Status = info.Status,
                    IpAddress = sourceIp,
                    LastUpdate = now
                };
                await RedisDatabase.HashSetAsync(CacheKeys.AgentInfo, info.Id, newInfo);

                Deployment activeDeployment = await RedisDatabase.HashGetAsync<Deployment>(CacheKeys.ActiveDeployments, info.Id);
                command.Deployment = activeDeployment;

                List<DeploymentResult> deploymentResults = await RedisDatabase.GetAsync<List<DeploymentResult>>(CacheKeys.DeploymentResults);
                List<string> successful = (deploymentResults ?? new List<DeploymentResult>())
                    .Where(i => i != null)
                    .Where(i => i.CommandResults.All(j => j.Success))
                    .Select(i => i.AgentInfo.ActiveDeploymentId)
                    .ToList();

                if (activeDeployment == null || successful.Contains(activeDeployment.Id) || agentInfo.ActiveDeploymentId == activeDeployment.Id)
                {
                    command.Execute = false;
                }

                return command;
            });

        [HttpPost("report")]
        public async Task<HttpActionResult<DeploymentResult, bool>> Report([FromBody] DeploymentResult deploymentResult) => await this.WithResponseContainer(
            deploymentResult,
            async result =>
            {
                string sourceIp = Request?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                result.AgentInfo.IpAddress = sourceIp;
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                result.AgentInfo.LastUpdate = now;

                List<DeploymentResult> deploymentResults = await RedisDatabase.GetAsync<List<DeploymentResult>>(CacheKeys.DeploymentResults);
                deploymentResults = deploymentResults ?? new List<DeploymentResult>();
                deploymentResults.Add(result);
                await RedisDatabase.ReplaceAsync(CacheKeys.DeploymentResults, deploymentResults);
                return true;
            });
    }
}
