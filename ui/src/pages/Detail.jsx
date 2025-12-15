import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";

export default function Detail() {
    const { id } = useParams();
    const [doc, setDoc] = useState(null);
    const [err, setErr] = useState("");

    useEffect(() => {
        DocumentsApi.get(id).then(setDoc).catch((e) => setErr(e.message));
    }, [id]);

    if (err)
        return <p className="text-center text-red-500 font-semibold">Error: {err}</p>;
    if (!doc)
        return <p className="text-center text-indigo-500 font-medium">Loading…</p>;

    const fileUrl = `http://localhost:8080/files/${doc.filePath}`;

    return (
        <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-8 max-w-4xl mx-auto">
            <h1 className="text-3xl font-semibold text-indigo-600 mb-4">{doc.title}</h1>

            <div className="space-y-2 mb-6">
                <p className="text-gray-600">
                    <strong>ID:</strong> {doc.id}
                </p>
                <p className="text-gray-600">
                    <strong>Uploaded:</strong> {new Date(doc.uploadedAt).toLocaleString()}
                </p>
            </div>

            <div className="my-6 p-4 bg-blue-50 rounded-lg border border-blue-200">
                <h3 className="font-bold text-blue-800">✨ AI Summary</h3>
                <p className="mt-2 text-gray-700 whitespace-pre-line">
                    {doc.summary ? doc.summary : "Generating summary..."}
                </p>
            </div>

            <div className="mb-8">
                <h2 className="text-lg font-medium text-gray-700 mb-3">PDF Preview</h2>
                <object
                    data={fileUrl}
                    type="application/pdf"
                    width="100%"
                    height="600px"
                    className="border border-gray-200 rounded-xl shadow-md"
                >
                    <p className="p-4 text-gray-500">
                        PDF preview not supported.{" "}
                        <a
                            href={fileUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-indigo-600 underline"
                        >
                            Open PDF
                        </a>
                    </p>
                </object>
            </div>

            <div className="text-right">
                <Link
                    to="/"
                    className="text-indigo-600 hover:text-indigo-800 font-semibold text-sm transition"
                >
                    ← Back to Dashboard
                </Link>
            </div>
        </div>
    );
}
