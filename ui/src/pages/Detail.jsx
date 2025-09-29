import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";

export default function Detail() {
    const { id } = useParams();
    const [doc, setDoc] = useState(null);
    const [err, setErr] = useState("");

    useEffect(() => {
        DocumentsApi.get(id).then(setDoc).catch(e => setErr(e.message));
    }, [id]);

    if (err) return <p style={{ color: "crimson" }}>Error: {err}</p>;
    if (!doc) return <p>Loading…</p>;

    return (
        <>
            <h1>{doc.title}</h1>
            <p><strong>Id:</strong> {doc.id}</p>
            <p><strong>Path:</strong> {doc.filePath}</p>
            <p><strong>Uploaded:</strong> {new Date(doc.uploadedAt).toLocaleString()}</p>
            <Link to="/">Back</Link>
        </>
    );
}
