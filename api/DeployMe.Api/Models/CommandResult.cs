namespace DeployMe.Api.Models
{
    public sealed class CommandResult
    {
        public bool Success { get; set; }
        public string Command { get; set; }
        public string Error { get; set; }
        public int? Code { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
    }
}
