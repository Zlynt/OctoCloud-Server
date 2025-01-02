import React, { createContext, useEffect, useState } from 'react';
import './App.css';
import Router from '../Router';
import { RouterProvider } from 'react-router-dom';
import LoadingPage from './routes/LoadingPage';
import Login from './routes/Login';

interface SettingsContextType {
    authenticated: boolean | undefined;
    setAuthenticated: React.Dispatch<React.SetStateAction<boolean | undefined>>;
    username: string;
    setUsername: React.Dispatch<React.SetStateAction<string>>;
}
export const SettingsContext = createContext<SettingsContextType | undefined>(undefined);

function App() {
    const [authenticated, setAuthenticated] = useState<boolean | undefined>();
    const [username, setUsername] = useState<string>("");

    useEffect(() => {
        const interval = setInterval(() => {
            if (authenticated === false) return;

            checkAuthentication();
        }, 5000);

        return () => clearInterval(interval);
    }, [authenticated]);

    const checkAuthentication = async () => {
        try {
            const response = await fetch('/Account/Current-User', {
                method: 'GET'
            });
            const data = await response.json();

            console.log(data);

            if (data.message === "No user is currently logged in") setAuthenticated(false);

            if (data.username) {
                setAuthenticated(true);
                setUsername(data.username);
            }
        } catch (error) {
            console.error('Network error:', error);
            setAuthenticated(false);
        }
    };


    return (
        <SettingsContext.Provider value={{
            authenticated, setAuthenticated,
            username, setUsername
        }}>
            {authenticated === undefined ?
                <LoadingPage /> : authenticated === true ?
                    <RouterProvider router={Router} /> :
                    <Login />
            }
        </SettingsContext.Provider>
    );
}


export default App;