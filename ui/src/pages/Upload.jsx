import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";

export default function Upload() {
    const [title, setTitle] = useState("");
    const [file, setFile] = useState(null);
    const [err, setErr] = useState("");
    const navigate = useNavigate();

    const submit = async (e) => {
        e.preventDefault();
        setErr("");

        if (!file) {
            setErr("Please select a PDF file.");
            return;
        }

        const formData = new FormData();
        formData.append("title", title);
        formData.append("file", file);

        try {
            await DocumentsApi.upload(formData);
            navigate("/");
        } catch (e) {
            setErr(e.message);
        }
    };

    return (
        <form onSubmit={submit} style={{ maxWidth: 480 }}>
            <h1>Upload</h1>
            {err && <p style={{ color: "crimson" }}>{err}</p>}
            <label>
                Title<br />
                <input
                    value={title}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                />
            </label>
            <br /><br />
            <label>
                PDF File<br />
                <input
                    type="file"
                    accept=".pdf"
                    onChange={(e) => setFile(e.target.files[0])}
                    required
                />
            </label>
            <br /><br />
            <button type="submit">Upload</button>
        </form>
    );
}
