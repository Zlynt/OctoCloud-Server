using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using MusicObj = OctoCloud.Server.Music.Music;
using MusicSettings = OctoCloud.Settings.Music;
using Microsoft.Extensions.Options;

namespace OctoCloud.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly MusicSettings _settings;

        public MusicController(IOptions<MusicSettings> settings) : base() {
            _settings = settings.Value;

            if (!Directory.Exists(_settings.Location)) Directory.CreateDirectory(_settings.Location);

            Console.WriteLine($"Music Folder Location: {_settings.Location}");
        }

        [HttpGet("")]
        public MusicObj[] Get() {
            //musicList.ToArray();
            List<MusicObj> musics = new List<MusicObj>();
            int counter = 0;
            foreach (string localFilePath in Directory.GetFiles(Path.GetFullPath(_settings.Location), "*", SearchOption.AllDirectories))
            {
                string remoteFilePath = localFilePath.Replace(Path.GetFullPath(_settings.Location), "/Music/files").Replace("\\", "/");
                Console.WriteLine(localFilePath);
                musics.Add(new MusicObj
                {
                    Id = counter+"",
                    Title = Path.GetFileNameWithoutExtension(remoteFilePath),
                    //Artists = [],
                    //Album = "",
                    //AlbumImageURL = "",
                    StreamUrl = remoteFilePath
                });
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
