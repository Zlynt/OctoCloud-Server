namespace OctoCloud.Settings
{
    public class Database
    {
        public required string Host { get; set; }
        public required int Port { get; set; }
        public required string Name { get; set; }
        public required string User { get; set; }
        public required string Password { get; set; }
    }
}