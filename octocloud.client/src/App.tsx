import { useEffect, useRef, useState } from 'react';
import './App.css';

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

function App() {
    const [musics, setMusics] = useState<Music[]>();
    const [currentSong, setCurrentSong] = useState<string>("");
    const audioRef = useRef<HTMLAudioElement>(null);
    
    useEffect(() => {
        populateMusicData();
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

    const contents = musics === undefined
        ? <p><em>Please wait while we load your experience.</em></p>
        : <>
             <audio controls ref={audioRef}>
                <source src={currentSong} type="audio/mpeg"/>
            </audio> 
            <table className="table table-striped" aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Artist</th>
                        <th>Album</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {musics.map(music =>
                        <tr key={music.Id}>
                            <td>{music.Title}</td>
                            <td>{music.Artists? music.Artists.map((artist: Artist) => artist.Name).join(", ") : "Unkown"}</td>
                            <td>{music.Album?.Name ?? "Unkown"}</td>
                            <td><a href="#" onClick={() => {
                                setCurrentSong(music.StreamUrl);
                                if(audioRef.current) {
                                    audioRef.current.src = music.StreamUrl;
                                    console.log("Playing: " + music.StreamUrl);
                                    //@ts-ignore
                                    audioRef.current.type = determineMimeType(music.StreamUrl);
                                    audioRef.current.play();
                                }
                            }}>Listen</a></td>
                        </tr>
                    )}
                </tbody>
            </table>
        </>;

    return (
        <div>
            <h1 id="tabelLabel">Tunes</h1>
            {contents}
        </div>
    );

    async function populateMusicData() {
        const response = await fetch('Music/List');
        const data = await response.json();
        setMusics(data);
    }
}

export default App;