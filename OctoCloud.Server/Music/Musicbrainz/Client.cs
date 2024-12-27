using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using MusicbrainzApiReturn = OctoCloud.Server.Musicbrainz.ApiReturn;

namespace OctoCloud.Server.Musicbrainz
    {
    public class Client
    {
        private string _ApiKey;

        public Client(string apiKey){
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
    }
}
