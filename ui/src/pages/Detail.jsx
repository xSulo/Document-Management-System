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

    const fileUrl = `http://localhost:8080/files/${doc.filePath}`;

    return (
        <div style={{ maxWidth: "800px", margin: "0 auto" }}>
            <h1>{doc.title}</h1>
            <p><strong>ID:</strong> {doc.id}</p>
            <p><strong>Uploaded:</strong> {new Date(doc.uploadedAt).toLocaleString()}</p>

            <h3>Preview</h3>
            <object
                data={fileUrl}
                type="application/pdf"
                width="100%"
                height="600px"
                style={{ border: "1px solid #ccc", borderRadius: "8px" }}
            >
                <p>
                    Your browser does not support PDF preview.{" "}
                    <a href={fileUrl} target="_blank" rel="noopener noreferrer">
                        Open PDF
                    </a>
                </p>
            </object>

            <br />
            <Link to="/">Back</Link>
        </div>
    );
}
