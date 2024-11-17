namespace OctoCloud.Server.Models
{
    public class MusicModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string AlbumImageURL { get; set; }
        public string[] Artists { get; set; }
        public string StreamUrl { get; set; }
    }
}
