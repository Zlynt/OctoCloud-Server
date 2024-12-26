import { useEffect, useState } from 'react';
import './App.css';

interface Music {
    id: string;
    title: string;
    album?: string;
    streamUrl: string;
    artists?: string[];
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
                    <tr key={music.id}>
                        <td>{music.title}</td>
                        <td>{music.artists? music.artists.join(",") : "Unkown"}</td>
                        <td>{music.album ?? "Unkown"}</td>
                        <td><a href={music.streamUrl}>Listen</a></td>
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
        const response = await fetch('Music');
        const data = await response.json();
        setMusics(data);
    }
}

export default App;