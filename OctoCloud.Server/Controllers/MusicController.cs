using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using Microsoft.Extensions.Options;
using OctoCloud.Server.Security;
using System.Text.Json;
// Music Settings
using MusicSettings = OctoCloud.Settings.Music;
// Music Fingerprinting
using Fingerprint = OctoCloud.Server.Clients.Musicbrainz.AcoustIDClient;
// Music
using MusicbrainzApiReturn = OctoCloud.Server.Clients.Musicbrainz.ApiReturn;
// MusicBrainz Client
using MusicbrainzClient = OctoCloud.Server.Clients.Musicbrainz.MusicbrainzClient;
using AudioFingerprint = OctoCloud.Server.Clients.Musicbrainz.AudioFingerprint;
using MusicbrainzResult = OctoCloud.Server.Clients.Musicbrainz.Result;
using MusicbrainzRecording = OctoCloud.Server.Clients.Musicbrainz.Recording;
using MusicbrainzArtist = OctoCloud.Server.Clients.Musicbrainz.Artist;
using MusicbrainzReleaseGroup = OctoCloud.Server.Clients.Musicbrainz.ReleaseGroup;
// Music Model
using MusicModel = OctoCloud.Server.Models.Music.Music;

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

        }

        //[Authorize]
        [HttpGet("List")]
        public async Task<IEnumerable<MusicModel>> List() {
            return MusicModel.GetAllMusic();
        }

        //[Authorize]
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
