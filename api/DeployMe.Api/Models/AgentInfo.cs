using System.Collections.Generic;
using DeployMe.Api.Extensions;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;

namespace DeployMe.Api.Models
{
    public sealed class AgentInfo
    {
        public DeploymentPackage DeploymentPackage { get; set; }
        public string ActiveDeploymentId { get; set; }
        public string IpAddress { get; set; }
        public List<string> ReportedIpAddresses { get; set; }
        public AgentStatus Status { get; set; }
        public long LastUpdate { get; set; }

        [JsonIgnore]
        public string Id => ReportedIpAddresses
            .OrderByOrdinal(i => i)
            .Join(",")
            .ToMD5();
    }
}
