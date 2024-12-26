import { useEffect, useState } from 'react';
import './App.css';

interface Music {
    id: string;
    title: string;
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
                </tr>
            </thead>
            <tbody>
                {musics.map(music =>
                    <tr key={music.id}>
                        <td>{music.title}</td>
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
        const response = await fetch('music');
        const data = await response.json();
        setMusics(data);
    }
}

export default App;