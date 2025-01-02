import { useContext, useState } from 'react';
import './index.css';
import { SettingsContext } from '../../App';

function Login() {
  const [userId, setUserId] = useState('');
  const [password, setPassword] = useState('');
  const settingsContext = useContext(SettingsContext);

  const [message, setMessage] = useState<{ message: string, type: "error" | "success" }>();

  const handleSubmit = (event: any) => {
    event.preventDefault();
    login();
  };

  const login = async () => {
    const payload = {
      username: userId,
      password: password,
    };

    try {
      const response = await fetch('/Account/Login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Login successful:', data);
        setMessage({ message: `Welcome ${userId}! Loading your experience...`, type: "success" });
        settingsContext?.setAuthenticated(true);
        // Handle successful login (e.g., redirect, save token)
      } else {
        console.error('Login failed:', response.statusText);
        // Handle login failure (e.g., show error message)
        if (response.statusText === "Unauthorized")
          setMessage({ message: 'There was an error with your User Id/Password combination. Please try again.', type: "error" });
        else
          setMessage({ message: response.statusText, type: "error" });
      }
    } catch (error) {
      console.error('Network error:', error);
      setMessage({ message: '' + error, type: "error" });
    }
  };

  if (!settingsContext) return;

  console.log(settingsContext);

  return (
    <div className="login-container">
      <h1>Tunes</h1>
      <p><em>Your music, on tunes.</em></p>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <input
            type="text"
            placeholder="Enter your user id"
            onChange={(evt) => setUserId(evt.target.value)}
            value={userId}
            required
          />
        </div>
        <div className="form-group">
          <input
            type="password"
            placeholder="Enter your password"
            onChange={(evt) => setPassword(evt.target.value)}
            value={password}
            required
          />
        </div>
        {message && (
          <p>{message.message}</p>
        )}
        <button type="submit">Login</button>
      </form>
    </div>
  );
}

export default Login;
