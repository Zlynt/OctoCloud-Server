namespace OctoCloud.Server.Music
{
    public class Music
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public string? Album { get; set; }
        public string? AlbumImageURL { get; set; }
        public string[]? Artists { get; set; }
        public required string StreamUrl { get; set; }
    }
}
