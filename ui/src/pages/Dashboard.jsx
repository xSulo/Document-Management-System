import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";
import Toast from "../components/Toast.jsx";

export default function Dashboard() {
    const [docs, setDocs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState("");
    const [success, setSuccess] = useState("");

    useEffect(() => {
        DocumentsApi.list()
            .then(setDocs)
            .catch((e) => setErr(e.message))
            .finally(() => setLoading(false));
    }, []);

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
