using System.Collections.Generic;
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
    public class AgentsController : Controller, ILogDelegate
    {
        public AgentsController(LogDelegate logDelegate, IRedisDatabase redisDatabase)
        {
            LogDelegate = logDelegate;
            RedisDatabase = redisDatabase;
        }

        private IRedisDatabase RedisDatabase { get; }

        public LogDelegate LogDelegate { get; }

        [HttpGet]
        public async Task<HttpActionResult<Dictionary<string, AgentInfo>>> List() => await this.WithResponseContainer(
            async () => await RedisDatabase.HashGetAllAsync<AgentInfo>(CacheKeys.AgentInfo));
    }
}
