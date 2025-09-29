import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import Dashboard from "./pages/Dashboard.jsx";
import Detail from "./pages/Detail.jsx";
import Upload from "./pages/Upload.jsx";

export default function App() {
    return (
        <BrowserRouter>
            <header style={{ padding: 12, borderBottom: "1px solid #eee" }}>
                <nav style={{ display: "flex", gap: 12 }}>
                    <Link to="/">Dashboard</Link>
                    <Link to="/upload">Upload</Link>
                </nav>
            </header>
            <main style={{ padding: 16 }}>
                <Routes>
                    <Route path="/" element={<Dashboard />} />
                    <Route path="/detail/:id" element={<Detail />} />
                    <Route path="/upload" element={<Upload />} />
                </Routes>
            </main>
        </BrowserRouter>
    );
}
