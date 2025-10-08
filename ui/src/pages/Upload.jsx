import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { DocumentsApi } from "../services/api.js";
import Toast from "../components/Toast.jsx";

export default function Upload() {
    const [title, setTitle] = useState("");
    const [file, setFile] = useState(null);
    const [err, setErr] = useState("");
    const [success, setSuccess] = useState("");
    const navigate = useNavigate();

    const submit = async (e) => {
        e.preventDefault();
        setErr("");
        setSuccess("");

        if (!file) {
            setErr("Please select a PDF file.");
            return;
        }

        const formData = new FormData();
        formData.append("title", title);
        formData.append("file", file);

        try {
            await DocumentsApi.upload(formData);
            setSuccess("✅ Document uploaded successfully!");
            setTimeout(() => navigate("/"), 2000); 
        } catch (e) {
            setErr(e.message);
        }
    };

    return (
        <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-10 max-w-lg mx-auto relative">
            <h1 className="text-3xl font-semibold mb-6 text-indigo-600">
                Upload New Document
            </h1>

            {err && (
                <p className="text-red-600 font-medium bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
                    {err}
                </p>
            )}

            <form onSubmit={submit} className="flex flex-col gap-6">
                <div>
                    <label className="block font-medium text-gray-700 mb-1">Title</label>
                    <input
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        className="border border-gray-300 rounded-lg w-full p-2 focus:ring-2 focus:ring-indigo-400 outline-none"
                        required
                    />
                </div>

                <div>
                    <label className="block font-medium text-gray-700 mb-1">PDF File</label>
                    <input
                        type="file"
                        accept=".pdf"
                        onChange={(e) => setFile(e.target.files[0])}
                        className="border border-gray-300 rounded-lg w-full p-2 bg-gray-50 focus:ring-2 focus:ring-indigo-300"
                        required
                    />
                </div>

                <button
                    type="submit"
                    className="bg-gradient-to-r from-indigo-500 to-cyan-500 text-white font-semibold py-2.5 rounded-lg shadow hover:from-indigo-600 hover:to-cyan-600 transition"
                >
                    Upload
                </button>
            </form>

            <Toast message={success} type="success" />
        </div>
    );
}
