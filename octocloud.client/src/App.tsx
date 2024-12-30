import { useEffect, useState } from 'react';
import './App.css';

interface Music {
    Id: string;
    Title: string;
    Album?: {
        Id: string;
        Name: string;
        ImageUrl: string;
    }
    Artists: {
        Id: string;
        Name: string;
    }
    StreamUrl: string;
}

function App() {
    const [musics, setMusics] = useState<Music[]>();

    useEffect(() => {
        populateMusicData();
    }, []);

    const contents = musics === undefined
        ? <p><em>Please wait while we load your experience.</em></p>
        : <table className="table table-striped" aria-labelledby="tabelLabel">
            <thead>
                <tr>
                    <th>Title</th>
                    <th>Artist</th>
                    <th>Album</th>
                    <th>URL</th>
                </tr>
            </thead>
            <tbody>
                {musics.map(music =>
                    <tr key={music.Id}>
                        <td>{music.Title}</td>
                        <td>{music.Artists? music.Artists.join(",") : "Unkown"}</td>
                        <td>{music.Album?.Name ?? "Unkown"}</td>
                        <td><a href={music.StreamUrl}>Listen</a></td>
                    </tr>
                )}
            </tbody>
        </table>;

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