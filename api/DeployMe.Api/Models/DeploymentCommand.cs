namespace DeployMe.Api.Models
{
    public sealed class DeploymentCommand
    {
        public Deployment Deployment { get; set; }
        public bool Execute { get; set; }
    }
}