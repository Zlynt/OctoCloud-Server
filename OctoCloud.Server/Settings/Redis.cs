namespace OctoCloud.Settings
{
    public class Redis
    {
        public required string Configuration { get; set; }
        public required string InstanceName { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}