using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using MusicbrainzApiReturn = OctoCloud.Server.Clients.Musicbrainz.ApiReturn;
using System.Text.Json.Serialization;

namespace OctoCloud.Server.Clients.Musicbrainz
    {
    public class MusicbrainzClient
    {
        private string _ApiKey;

        public MusicbrainzClient(string apiKey){
            this._ApiKey = apiKey;
        }

        private readonly HttpClient client = new HttpClient();

        public async Task<string> GetAlbumImageAsync(string mbid)
        {
            string url = $"https://coverartarchive.org/release-group/{mbid}/front";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<MusicbrainzApiReturn> GetInfoFromFingerprint(string fingerprint, int duration){
            string meta = "recordings+releasegroups+compress";
            string url = $"https://api.acoustid.org/v2/lookup?client={this._ApiKey}&meta={meta}&duration={duration}&fingerprint={fingerprint}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            MusicbrainzApiReturn apiReturn = JsonSerializer.Deserialize<MusicbrainzApiReturn>(responseBody);

            if(apiReturn.Status != "ok") throw new Exception("API didn't return 'ok' status");

            return apiReturn;

        }

        public async Task<MusicMatch> GetBestMatchFromFingerprint(string fingerprint, int duration) {
            MusicbrainzApiReturn apiReturn = await GetInfoFromFingerprint(fingerprint, duration);

            MusicMatch musicMatch = null;

            foreach(Result result in apiReturn.Results){
                // If the list is empty or null, skip
                if(result.Recordings == null || !result.Recordings.Any()) continue;

                foreach(Recording recording in result.Recordings) {
                    // If there isnt a recording with the same duration, return the first recording match
                    // In other words, if we already found a match, check if there is a result whose duration is the same as our file
                    if(musicMatch != null && recording.Duration != duration) continue;
                    if(recording.Duration == 0) continue; // If duration is 0 (which is impossible), do not use this recording


                    musicMatch = new MusicMatch();
                    musicMatch.MusicbrainzApiReturn = apiReturn;

                    musicMatch.Recording = recording;
                    // Title
                    musicMatch.Title = recording.Title;
                    // Id
                    musicMatch.Id = recording.Id;

                    // Parse music album
                    if(recording.ReleaseGroups == null || !recording.ReleaseGroups.Any()) continue;
                    ReleaseGroup releaseGroup = recording.ReleaseGroups .FirstOrDefault(rg => rg.Type == "Single") ?? 
                                                recording.ReleaseGroups.FirstOrDefault();

                    if (releaseGroup != null) {
                        musicMatch.ReleaseGroup = releaseGroup;
                        // Album Name
                        musicMatch.Album = releaseGroup.Title; 
                        // Album Image
                        musicMatch.AlbumImageURL = $"https://coverartarchive.org/release-group/{releaseGroup.Id}/front"; 
                    }
        
                    // Parse Artists
                    LinkedList<Artist> artists = new LinkedList<Artist>();
                    if(recording.Artists == null || !recording.Artists.Any()) continue;
                    foreach(Artist artist in recording.Artists){
                        artists.AddLast(artist);
                    }
                    musicMatch.Artists = artists.ToArray<Artist>();
                    
                }
            }
            
            if(musicMatch == null) throw new Exception("Music not found");

            return musicMatch;
        }
    }

    public class MusicMatch
    {
        public string Id;
        public string Title;
        public string Album;
        public string AlbumImageURL;
        public Artist[] Artists;
        public MusicbrainzApiReturn MusicbrainzApiReturn;
        public Recording Recording;
        public ReleaseGroup ReleaseGroup;

    }

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
        public string? Title { get; set; } 

        [JsonPropertyName("artists")]
        public List<Artist>? Artists { get; set; } 

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
