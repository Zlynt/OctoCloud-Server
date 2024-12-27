
using System.Text.Json.Serialization;

namespace OctoCloud.Server.Musicbrainz
{
    public class Artist
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; } 

        [JsonPropertyName("joinphrase")]
        public string? Joinphrase { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
    public class ReleaseGroup {
        [JsonPropertyName("id")]
        public string Id { get; set; } 

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("secondarytypes")]
        public List<string> Secondarytypes { get; set; } 
    }
    public class Recording { 
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } 

        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; } 

        [JsonPropertyName("duration")]
        public int Duration { get; set; } 

        [JsonPropertyName("releasegroups")]
        public List<ReleaseGroup> ReleaseGroups { get; set; } 
    }
    public class Result {
        [JsonPropertyName("id")]
        public string Id { get; set; } 

        [JsonPropertyName("recordings")]
        public List<Recording> Recordings { get; set; }
    } 
    public class ApiReturn { 
        [JsonPropertyName("results")]
        public List<Result> Results { get; set; } 

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}