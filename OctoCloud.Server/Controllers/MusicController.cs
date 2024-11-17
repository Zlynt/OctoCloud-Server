﻿using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using OctoCloud.Server.Models;

namespace OctoCloud.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private static string fileStorageLocation = Environment.GetEnvironmentVariable("MUSIC_FOLDER") ?? "./music";

        private static readonly IEnumerable<MusicModel> musicList = new[]
        {
            new MusicModel{
                Id="1",
                Title = "Fly Away (INUKSHUK REMIX)",
                Artists=["THEFATRAT", "ANJULIE"],
                Album="Fly Away",
                AlbumImageURL="https://static.wixstatic.com/media/1f923f_96bfedf75d25413baba7427d16ac0692~mv2.jpg/v1/fill/w_1920,h_1080,al_c,q_90/TheFatRat%20-%20Fly%20Away.jpg",
                FilePath="https://static.wixstatic.com/mp3/80cc5d_40909e0b38bd44c49576164e34d29e85.mp3?dn=FLY%20AWAY%20INUKSHUK%20REMIX.mp3"
            }
        };

        [HttpGet("")]
        public MusicModel[] Get() => musicList.ToArray();

        [HttpGet("files/{*fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                fileName = WebUtility.UrlDecode(fileName);
                var filePath = Path.Combine(fileStorageLocation, "files", fileName);
                if (!System.IO.File.Exists(filePath)) { return NotFound(); }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                var fileType = "application/octet-stream";
                var result = new FileStreamResult(memory, fileType)
                {
                    FileDownloadName = Path.GetFileName(filePath)
                };
                Response.Headers.Add("Content-Disposition", new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(filePath)
                }.ToString());

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while downloading file {fileName}: {e.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
