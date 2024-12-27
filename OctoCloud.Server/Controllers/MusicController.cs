using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using MusicObj = OctoCloud.Server.Music.Music;
using MusicSettings = OctoCloud.Settings.Music;
using Fingerprint = OctoCloud.Server.Music.Fingerprint;
using Microsoft.Extensions.Options;
using OctoCloud.Server.Security;
using OctoCloud.Server.Music;

using MusicbrainzApiReturn = OctoCloud.Server.Musicbrainz.ApiReturn;
using MusicbrainzClient = OctoCloud.Server.Musicbrainz.Client;
using System.Text.Json;
using OctoCloud.Server.Musicbrainz;

namespace OctoCloud.Server.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly MusicSettings _settings;
        private Fingerprint _fingerprint;
        private MusicbrainzClient _mbClient;

        public MusicController(IOptions<MusicSettings> settings) : base() {
            _settings = settings.Value;
            _fingerprint = new Fingerprint();
            _mbClient = new MusicbrainzClient(_settings.ApiKey);

            if (!Directory.Exists(_settings.Location)) Directory.CreateDirectory(_settings.Location);

            Console.WriteLine($"Music Folder Location: {_settings.Location}");
        }

        [HttpGet("")]
        public async Task<IEnumerable<MusicObj>> Get() {
            List<MusicObj> musics = new List<MusicObj>();
            int counter = 0;
            foreach (string localFilePath in Directory.GetFiles(Path.GetFullPath(_settings.Location), "*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(localFilePath).ToLowerInvariant();
                if (extension != ".mp3" && extension != ".wav" && extension != ".ogg" && extension != ".flac") { 
                    continue;
                }

                string remoteFilePath = localFilePath.Replace(Path.GetFullPath(_settings.Location), "/Music/files").Replace("\\", "/");
                Console.WriteLine(localFilePath);

                AudioFingerprint musicFingerprint = _fingerprint.GetFingerprint($"\"{localFilePath}\"");
                
                MusicObj musicObj = new MusicObj
                {
                    Id = Hash.GetHash(musicFingerprint.ToString()),
                    Title = Path.GetFileNameWithoutExtension(remoteFilePath),
                    //Artists = [],
                    //Album = "",
                    //AlbumImageURL = "",
                    StreamUrl = remoteFilePath
                };
                try{
                    MusicbrainzApiReturn apiReturn = await _mbClient.GetInfoFromFingerprint(musicFingerprint.Fingerprint, musicFingerprint.Duration);

                    Console.WriteLine(apiReturn.Results);
                    foreach(Result result in apiReturn.Results){
                        // If the list is empty skip
                        if(!result.Recordings.Any()) continue;

                        foreach(Recording recording in result.Recordings) {
                            if(recording.Duration != musicFingerprint.Duration) continue;
                            // Title
                            musicObj.Title = recording.Title;
                            // Id
                            musicObj.Id = recording.Id;
                            // Artists
                            LinkedList<string> artists = new LinkedList<string>();


                            // If the list is empty skip
                            if(!recording.Artists.Any()) continue;
                            foreach(Artist artist in recording.Artists){
                                artists.AddLast(artist.Name);
                            }
                            musicObj.Artists = artists.ToArray<string>();

                            ReleaseGroup releaseGroup = recording.ReleaseGroups .FirstOrDefault(rg => rg.Type == "Single") ?? 
                                                        recording.ReleaseGroups.FirstOrDefault();

                            if (releaseGroup != null) { 
                                // Album Name
                                musicObj.Album = releaseGroup.Title; 
                                // Album Image
                                musicObj.AlbumImageURL = $"https://coverartarchive.org/release-group/{releaseGroup.Id}/front"; 
                            }
                            
                        }
                    }
                } catch (HttpRequestException e) { 
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message: {0}", e.Message);
                } finally {
                    musics.Add(musicObj);
                }
                counter++;
            }
            return musics.ToArray();
        }

        [HttpGet("files/{*fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.GetFullPath(Path.Combine(_settings.Location, fileName));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".mp3": extension = "audio/mpeg"; break;
                case ".wav": extension = "audio/wav"; break;
                case ".ogg": extension = "audio/ogg"; break;
                case ".flac": extension = "audio/flac"; break;
                default: extension = "application/octet-stream"; break;
            }

            return PhysicalFile(filePath, extension, Path.GetFileName(filePath));
        }
    }
}
