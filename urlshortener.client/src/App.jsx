import { useState } from 'react';

function App() {
    const [urlInput, setUrlInput] = useState('');
    const [shortUrl, setShortUrl] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const BACKEND_URL = "http://localhost:7070";

    const handleCreate = async () => {
        setLoading(true);
        setError('');
        setShortUrl('');

        try {
            const response = await fetch(`${BACKEND_URL}/api/urls`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ url: urlInput })
            });

            const data = await response.json();

            if (response.ok) {
                setShortUrl(data.shortUrl);
            } else {
                setError(data.message || "Something went wrong!");
            }
        } catch {
            setError("Cannot connect to server!");
        } finally {
        setLoading(false);
        }
    };

    const handleCopy = () => {
        navigator.clipboard.writeText(shortUrl);
        alert("Copied to clipboard!");
    };

    return (
        <div style={styles.container}>
            <h1 style={styles.title}> URL Shortener</h1>

            <div style={styles.card}>
                <input
                    type="text"
                    value={urlInput}
                    onChange={(e) => setUrlInput(e.target.value)}
                    placeholder="Enter your URL..."
                    style={styles.input}
                />

                <button onClick={handleCreate} style={styles.button} disabled={loading}>
                    {loading ? "Processing..." : "Shorten"}
                </button>

                {/* Error UI */}
                {error && <p style={styles.error}>{error}</p>}

                {/* Result UI */}
                {shortUrl && (
                    <div style={styles.result}>
                        <a href={shortUrl} target="_blank" rel="noreferrer">
                            {shortUrl}
                        </a>
                        <button onClick={handleCopy} style={styles.copyBtn}>
                            Copy
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}

//  Simple modern styles
const styles = {
    container: {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        marginTop: '100px',
        fontFamily: 'Arial'
    },
    title: {
        marginBottom: '20px'
    },
    card: {
        padding: '30px',
        borderRadius: '10px',
        boxShadow: '0 4px 10px rgba(0,0,0,0.1)',
        textAlign: 'center',
        width: '400px'
    },
    input: {
        width: '100%',
        padding: '10px',
        marginBottom: '15px',
        borderRadius: '5px',
        border: '1px solid #ccc'
    },
    button: {
        padding: '10px 20px',
        border: 'none',
        backgroundColor: '#007bff',
        color: 'white',
        borderRadius: '5px',
        cursor: 'pointer'
    },
    result: {
        marginTop: '20px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
    },
    copyBtn: {
        marginLeft: '10px',
        padding: '5px 10px',
        border: 'none',
        backgroundColor: 'green',
        color: 'white',
        borderRadius: '5px',
        cursor: 'pointer'
    },
    error: {
        color: 'red',
        marginTop: '10px'
    }
};

export default App;