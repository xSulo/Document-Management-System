import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";

export default function Dashboard() {
    const [docs, setDocs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState("");

    useEffect(() => {
        DocumentsApi.list()
            .then(setDocs)
            .catch(e => setErr(e.message))
            .finally(() => setLoading(false));
    }, []);

    const onDelete = async (id) => {
        if (!confirm("Delete document?")) return;
        await DocumentsApi.delete(id);
        setDocs(prev => prev.filter(d => d.id !== id));
    };

    if (loading) return <p>Loading…</p>;
    if (err) return <p style={{ color: "crimson" }}>Error: {err}</p>;

    return (
        <>
            <h1>Documents</h1>
            <table cellPadding="8" style={{ borderCollapse: "collapse", width: "100%" }}>
                <thead>
                    <tr><th>Id</th><th>Title</th><th>Uploaded</th><th>Actions</th></tr>
                </thead>
                <tbody>
                    {docs.map(d => (
                        <tr key={d.id} style={{ borderTop: "1px solid #eee" }}>
                            <td>{d.id}</td>
                            <td>{d.title}</td>
                            <td>{new Date(d.uploadedAt).toLocaleString()}</td>
                            <td>
                                <Link to={`/detail/${d.id}`} style={{ marginRight: 8 }}>Details</Link>
                                <button onClick={() => onDelete(d.id)}>Delete</button>
                            </td>
                        </tr>
                    ))}
                    {docs.length === 0 && (
                        <tr><td colSpan="4" style={{ textAlign: "center", color: "#777" }}>No documents</td></tr>
                    )}
                </tbody>
            </table>
        </>
    );
}
