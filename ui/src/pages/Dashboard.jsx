import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";
import Toast from "../components/Toast.jsx";

export default function Dashboard() {
    const [docs, setDocs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState("");
    const [success, setSuccess] = useState("");
    const [searchQuery, setSearchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const [searchError, setSearchError] = useState('');

    useEffect(() => {
        DocumentsApi.list()
            .then(setDocs)
            .catch((e) => setErr(e.message))
            .finally(() => setLoading(false));
    }, []);

    const handleSearch = async () => {
        if (!searchQuery) return;

        try {
            setSearchError('');
            const response = await fetch(`/api/search?query=${encodeURIComponent(searchQuery)}`);
            if (!response.ok) throw new Error('Error when searching');

            const data = await response.json();
            setSearchResults(data);
        } catch (err) {
            setSearchError(err.message);
            setSearchResults([]);
        }
    };

    const onDelete = async (id) => {
        if (!confirm("Delete document?")) return;
        await DocumentsApi.delete(id);
        setDocs((prev) => prev.filter((d) => d.id !== id));
        setSuccess("🗑️ Document deleted successfully!");
        setTimeout(() => setSuccess(""), 3000);
    };

    if (loading)
        return <p className="text-center text-indigo-500 font-medium">Loading…</p>;
    if (err)
        return <p className="text-center text-red-500 font-semibold">Error: {err}</p>;

    return (
        <div className="bg-white shadow-xl rounded-2xl p-8 border border-gray-100 relative">
            <h1 className="text-3xl font-bold text-indigo-600 mb-8 flex items-center gap-2">
                📚 Documents
            </h1>


            <div style={{ margin: '20px 0', padding: '20px', backgroundColor: '#f8f9fa', borderRadius: '8px' }}>
                <h3>🔍 Document Search</h3>
                <div style={{ display: 'flex', gap: '10px' }}>
                    <input
                        type="text"
                        placeholder="Enter terms..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
                        style={{ flex: 1, padding: '10px', border: '1px solid #ccc', borderRadius: '4px' }}
                    />
                    <button
                        onClick={handleSearch}
                        style={{ padding: '10px 20px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
                    >
                        Search
                    </button>
                </div>

                {searchError && <p style={{ color: 'red', marginTop: '10px' }}>{searchError}</p>}

                <div style={{ marginTop: '15px' }}>
                    {searchResults.length > 0 ? (
                        <ul style={{ listStyle: 'none', padding: 0 }}>
                            {searchResults.map((doc) => (
                                <li key={doc.documentId} style={{ background: 'white', border: '1px solid #ddd', marginBottom: '5px', padding: '10px', borderRadius: '4px' }}>
                                    <strong>📄 {doc.title || 'Ohne Titel'}</strong>
                                    <span style={{ color: '#888', fontSize: '0.8em', marginLeft: '10px' }}>(ID: {doc.documentId})</span>
                                    <a href={`/detail/${doc.documentId}`} style={{ float: 'right', textDecoration: 'none', color: '#007bff' }}>Öffnen →</a>
                                </li>
                            ))}
                        </ul>
                    ) : (
                        searchQuery && !searchError //&& <p style={{ color: '#666' }}>No results.</p>
                    )}
                </div>
            </div>


            <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                    <thead className="bg-indigo-50 text-indigo-700 text-sm uppercase tracking-wide">
                        <tr>
                            <th className="py-3 px-4">ID</th>
                            <th className="py-3 px-4">Title</th>
                            <th className="py-3 px-4">Uploaded</th>
                            <th className="py-3 px-4 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {docs.length > 0 ? (
                            docs.map((d) => (
                                <tr
                                    key={d.id}
                                    className="border-t hover:bg-indigo-50 transition-colors"
                                >
                                    <td className="py-3 px-4">{d.id}</td>
                                    <td className="py-3 px-4 font-medium">{d.title}</td>
                                    <td className="py-3 px-4 text-gray-600">
                                        {new Date(d.uploadedAt).toLocaleString()}
                                    </td>
                                    <td className="py-3 px-4 text-right space-x-3">
                                        <Link
                                            to={`/detail/${d.id}`}
                                            className="text-indigo-600 hover:text-indigo-800 font-semibold transition"
                                        >
                                            Details
                                        </Link>
                                        <button
                                            onClick={() => onDelete(d.id)}
                                            className="text-rose-500 hover:text-rose-700 font-medium transition"
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan="4" className="text-center text-gray-400 py-6 italic">
                                    No documents available.
                                </td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            <Toast message={success} type="success" />
        </div>
    );
}
