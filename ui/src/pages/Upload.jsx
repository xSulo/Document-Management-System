import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";

export default function Upload() {
    const [title, setTitle] = useState("");
    const [filePath, setFilePath] = useState("");
    const [err, setErr] = useState("");
    const navigate = useNavigate();

    const submit = async (e) => {
        e.preventDefault();
        setErr("");
        try {
            await DocumentsApi.create({ title, filePath });
            navigate("/");
        } catch (e) {
            setErr(e.message);
        }
    };

    return (
        <form onSubmit={submit} style={{ maxWidth: 480 }}>
            <h1>Upload</h1>
            {err && <p style={{ color: "crimson" }}>{err}</p>}
            <label>Title<br />
                <input value={title} onChange={e => setTitle(e.target.value)} required />
            </label><br /><br />
            <label>File Path<br />
                <input value={filePath} onChange={e => setFilePath(e.target.value)} required />
            </label><br /><br />
            <button type="submit">Create</button>
        </form>
    );
}
