using System.Collections.Generic;

namespace DeployMe.Api.Models
{
    public sealed class DeploymentRequest
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Yaml { get; set; }
        public List<string> Agents { get; set; }
    }
}
