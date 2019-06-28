using System.Collections.Generic;

namespace DeployMe.Api.Models
{
    public sealed class Deployment
    {
        public DeploymentPackage DeploymentPackage { get; set; }
        public List<string> InstallCommands { get; set; }
        public List<string> StartCommands { get; set; }
        public List<string> StopCommands { get; set; }
        public List<string> UninstallCommands { get; set; }
        public long Timestamp { get; set; }
        public string Id { get; set; }
    }
}
