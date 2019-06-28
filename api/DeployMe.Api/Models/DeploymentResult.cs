using System.Collections.Generic;

namespace DeployMe.Api.Models
{
    public sealed class DeploymentResult
    {
        public AgentInfo AgentInfo { get; set; }
        public List<CommandResult> CommandResults { get; set; }
    }
}
