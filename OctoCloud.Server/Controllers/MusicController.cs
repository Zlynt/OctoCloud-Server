using Microsoft.AspNetCore.Mvc;
using OctoCloud.Server.Models;

namespace OctoCloud.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
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
    }
}
