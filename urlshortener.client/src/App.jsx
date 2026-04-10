import { useState } from 'react';

function App() {
    // Define states for input, result, loading, and error messages
    const [urlInput, setUrlInput] = useState('');
    const [shortUrl, setShortUrl] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Backend API base URL (uses env variable or defaults to empty string for proxy)
    const BACKEND_URL = import.meta.env.VITE_API_URL || "";

    // Function to handle the creation of a short URL
    const handleCreate = async () => {
        setLoading(true);
        setError('');
        setShortUrl('');

        try {
            // Debugging: Logging the full API URL being used
            console.log("Fetching from:", `${BACKEND_URL}/api/urls`);

            // Send POST request to the backend with the original URL
            const response = await fetch(`${BACKEND_URL}/api/urls`,  {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ url: urlInput })
            });

            const data = await response.json();

            if (response.ok) {
                // Success: Update shortUrl state with data from server
                setShortUrl(data.shortUrl);
            } else {
                // Handle server-side validation errors or failures
                setError(data.message || "Something went wrong!");
            }
        } catch {
            // Handle network issues or server connection errors
            setError("Cannot connect to server!");
        } finally {
            // Reset loading state regardless of the outcome
            setLoading(false);
        }
    };

    // Helper function to copy the result to the system clipboard
    const handleCopy = () => {
        navigator.clipboard.writeText(shortUrl);
        alert("Copied to clipboard!");
    };

    return (
        <div style={styles.container}>
            <h1 style={styles.title}> URL Shortener</h1>

            <div style={styles.card}>
                {/* Controlled input field for the long URL */}
                <input
                    type="text"
                    value={urlInput}
                    onChange={(e) => setUrlInput(e.target.value)}
                    placeholder="Enter your URL..."
                    style={styles.input}
                />

                {/* Disable button during processing to prevent multiple requests */}
                <button onClick={handleCreate} style={styles.button} disabled={loading}>
                    {loading ? "Processing..." : "Shorten"}
                </button>

                {/* Conditional rendering for Error UI */}
                {error && <p style={styles.error}>{error}</p>}

                {/* Conditional rendering for Result UI using shortUrl state */}
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

// Simple modern styles using CSS-in-JS object
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