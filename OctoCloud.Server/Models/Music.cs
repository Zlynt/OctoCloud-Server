namespace OctoCloud.Server.Models
{
    public class Music
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string[] Artists { get; set; }
        public string FilePath { get; set; }
    }
}
