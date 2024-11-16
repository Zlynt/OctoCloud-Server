using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using OctoCloud.Server.Models;
using System.IO;

namespace OctoCloud.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private static string fileStorageLocation = Environment.GetEnvironmentVariable("MUSIC_FOLDER") ?? "./music";

        public MusicController() : base() {
            if (!Directory.Exists(fileStorageLocation)) Directory.CreateDirectory(fileStorageLocation);

            Console.WriteLine($"Music Folder Location: {fileStorageLocation}");
        }

        private static readonly IEnumerable<MusicModel> musicList = new[]
        {
            new MusicModel{
                Id="1",
                Title = "Fly Away (INUKSHUK REMIX)",
                Artists=["THEFATRAT", "ANJULIE"],
                Album="Fly Away",
                AlbumImageURL="https://static.wixstatic.com/media/1f923f_96bfedf75d25413baba7427d16ac0692~mv2.jpg/v1/fill/w_1920,h_1080,al_c,q_90/TheFatRat%20-%20Fly%20Away.jpg",
                StreamUrl="https://static.wixstatic.com/mp3/80cc5d_40909e0b38bd44c49576164e34d29e85.mp3?dn=FLY%20AWAY%20INUKSHUK%20REMIX.mp3"
            }
        };

        [HttpGet("")]
        public MusicModel[] Get() {
            //musicList.ToArray();
            List<MusicModel> musics = new List<MusicModel>();
            int counter = 0;
            foreach (string localFilePath in Directory.GetFiles(Path.GetFullPath(fileStorageLocation), "*", SearchOption.AllDirectories))
            {
                string remoteFilePath = localFilePath.Replace(Path.GetFullPath(fileStorageLocation), "/Music/files").Replace("\\", "/");
                Console.WriteLine(localFilePath);
                musics.Add(new MusicModel
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
        public async Task<IActionResult> Download(string fileName)
        {
            var filePath = Path.GetFullPath(Path.Combine(fileStorageLocation, fileName));

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            return PhysicalFile(filePath, "application/octet-stream", Path.GetFileName(filePath));
        }
    }
}
