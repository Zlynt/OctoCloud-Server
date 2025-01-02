import { useContext, useEffect, useRef, useState } from "react";
import { SettingsContext } from "../../App";

import './index.css';

interface Artist {
  Id: string;
  Name: string;
}
interface Album {
  Id: string;
  Name: string;
  ImageUrl: string;
}
interface Music {
  Id: string;
  Title: string;
  Album?: Album;
  Artists: Array<Artist>;
  StreamUrl: string;
}

function Home() {
  const settingsContext = useContext(SettingsContext);

  const [musics, setMusics] = useState<Music[]>([]);
  const [currentSong, setCurrentSong] = useState<{
    title: string,
    artists: string,
    album: string,
    albumImage: string,
    url: string
  }>();
  const audioRef = useRef<HTMLAudioElement>(null);

  useEffect(() => {
    const interval = setInterval(() => {
      console.log("Fetch Music List");
      populateMusicData();
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  const determineMimeType = (filePath: string) => {
    const extension = filePath.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'mp3': return 'audio/mpeg';
      case 'wav': return 'audio/wav';
      case 'ogg': return 'audio/ogg';
      case 'flac': return 'audio/flac';
      default: return 'audio/mpeg';
    }
  };

  async function populateMusicData() {
    const response = await fetch('/Music/List', {
      method: 'GET'
    });
    const data = await response.json();
    console.log(data);
    setMusics(data);
  }

  return (<>
    {/* This code is messy and not organized. This is just a prototype for functionality */}
    <div>
      <div>
        {/* Barra lateral */}
        <div style={{
          backgroundColor: "grey",
          position: "absolute",
          top: "0px",
          left: "0px",
          bottom: "10vh",
          width: "20vw",
          border: "1px solid black",
          overflowY: "scroll",
          overflowX: "hidden",
          fontSize: "0.5rem"
        }}>
          <ul style={{ listStyle: "none" }}>
            {musics.map(music =>
              <li className="musicList"
                onClick={() => {
                  setCurrentSong({
                    url: music.StreamUrl,
                    title: music.Title,
                    album: music.Album?.Name ?? "Unkown",
                    albumImage: music.Album?.ImageUrl ?? "",
                    artists: music.Artists ? music.Artists.map((artist: Artist) => artist.Name).join(", ") : "Unkown"
                  });
                  if (audioRef.current) {
                    audioRef.current.src = music.StreamUrl;
                    console.log("Playing: " + music.StreamUrl);
                    //@ts-ignore
                    audioRef.current.type = determineMimeType(music.StreamUrl);
                    audioRef.current.play();
                  }
                }}
              >{music.Artists ? music.Artists.map((artist: Artist) => artist.Name).join(", ") : "Unkown"} - {music.Title}</li>
            )}
          </ul>
        </div>
        {/* Conteudo */}
        <div>
          <h1>Development UI</h1>
          <h2>This will not be the final interface.</h2>
          <img src={currentSong?.albumImage} style={{ width: "50px", height: "50px" }} />
          <h4>Album: {currentSong?.album}</h4>
          <h3>{currentSong?.artists} - {currentSong?.title}</h3>
        </div>
      </div>
      {/* Barra de baixo */}
      <div style={{
        backgroundColor: "grey",
        position: "absolute",
        bottom: "0px",
        left: "0px",
        width: "100%",
        height: "10vh"
      }}>
        <audio controls ref={audioRef}>
          <source src={currentSong?.url} type="audio/mpeg" />
        </audio>
      </div>

    </div>
  </>);
}

export default Home;
