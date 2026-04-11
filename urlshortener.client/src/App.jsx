import { useState } from 'react';

// Main React component for the URL Shortener frontend application.
function App() {
    // State hooks to manage user input, API responses, UI loading states, and error alerts.
    const [urlInput, setUrlInput] = useState('');
    const [shortUrl, setShortUrl] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    // Configure the Backend API base URL. 
    // Uses VITE_API_URL from .env file during production, or an empty string for development proxying.
    const BACKEND_URL = import.meta.env.VITE_API_URL || "";

    // Handler to process URL shortening by calling the backend API.
    const handleCreate = async () => {
        setLoading(true); // Indicate processing has started
        setError('');     // Clear previous errors
        setShortUrl('');  // Reset previous results

        try {
            // Log the endpoint for debugging connectivity issues.
            console.log("Fetching from:", `${BACKEND_URL}/api/urls`);

            // Execute POST request with the long URL in the JSON body.
            const response = await fetch(`${BACKEND_URL}/api/urls`,  {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ url: urlInput })
            });

            const data = await response.json();

            if (response.ok) {
                // Update state with the shortened URL returned by the server.
                setShortUrl(data.shortUrl);
            } else {
                // Capture and display server-side validation or logic errors.
                setError(data.message || "Something went wrong!");
            }
        } catch (err) {
            // Handle network failures or unreachable server.
            setError(`Cannot connect to server at ${BACKEND_URL || window.location.origin}/api/urls`);
            console.error(err);
        } finally {
            // Ensure loading state is turned off regardless of success or failure.
            setLoading(false);
        }
    };

    // Helper function to copy the generated short URL to the device's clipboard.
    const handleCopy = () => {
        navigator.clipboard.writeText(shortUrl);
        alert("Copied to clipboard!");
    };

    return (
        <div style={styles.container}>
            <h1 style={styles.title}> URL Shortener</h1>

            <div style={styles.card}>
                {/* Controlled Input: Updates urlInput state as the user types. */}
                <input
                    type="text"
                    value={urlInput}
                    onChange={(e) => setUrlInput(e.target.value)}
                    placeholder="Enter your URL..."
                    style={styles.input}
                />

                {/* Submit Button: Triggers handleCreate and disables itself during loading. */}
                <button onClick={handleCreate} style={styles.button} disabled={loading}>
                    {loading ? "Processing..." : "Shorten"}
                </button>

                {/* Error Banner: Only displayed if the error state is not empty. */}
                {error && <p style={styles.error}>{error}</p>}

                {/* Result Section: Displayed only after a successful short URL generation. */}
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

// Inline CSS-in-JS object to define a clean, centered, and modern UI layout.
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