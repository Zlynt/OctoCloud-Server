﻿using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

using MusicObj = OctoCloud.Server.Music.Music;

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

        [HttpGet("")]
        public MusicObj[] Get() {
            //musicList.ToArray();
            List<MusicObj> musics = new List<MusicObj>();
            int counter = 0;
            foreach (string localFilePath in Directory.GetFiles(Path.GetFullPath(fileStorageLocation), "*", SearchOption.AllDirectories))
            {
                string remoteFilePath = localFilePath.Replace(Path.GetFullPath(fileStorageLocation), "/Music/files").Replace("\\", "/");
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
            var filePath = Path.GetFullPath(Path.Combine(fileStorageLocation, fileName));

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
