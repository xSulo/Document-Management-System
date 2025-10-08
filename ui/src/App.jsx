import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import Dashboard from "./pages/Dashboard.jsx";
import Detail from "./pages/Detail.jsx";
import Upload from "./pages/Upload.jsx";

export default function App() {
    return (
        <BrowserRouter>
            <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-cyan-50 text-gray-800 flex flex-col">
                {/* 🌟 Navbar */}
                <header className="bg-white/80 backdrop-blur-md shadow-sm border-b border-gray-100 sticky top-0 z-40">
                    <nav className="container mx-auto flex justify-between items-center px-8 py-4">
                        <h1 className="text-2xl font-extrabold text-indigo-600 tracking-tight">
                            DMS Cloud
                        </h1>
                        <div className="flex gap-6">
                            {[
                                { to: "/", label: "Dashboard" },
                                { to: "/upload", label: "Upload" },
                            ].map(({ to, label }) => (
                                <NavLink
                                    key={to}
                                    to={to}
                                    className={({ isActive }) =>
                                        `font-medium text-sm tracking-wide transition-all ${isActive
                                            ? "text-indigo-600 border-b-2 border-indigo-600 pb-1"
                                            : "text-gray-600 hover:text-indigo-500"
                                        }`
                                    }
                                >
                                    {label}
                                </NavLink>
                            ))}
                        </div>
                    </nav>
                </header>

                {/* 📄 Page content */}
                <main className="flex-grow container mx-auto px-6 py-10">
                    <Routes>
                        <Route path="/" element={<Dashboard />} />
                        <Route path="/detail/:id" element={<Detail />} />
                        <Route path="/upload" element={<Upload />} />
                    </Routes>
                </main>

                {/* 👣 Footer */}
                <footer className="bg-white border-t border-gray-200 py-4 text-center text-sm text-gray-500">
                    © {new Date().getFullYear()} DMS Cloud — Manage Smarter.
                </footer>
            </div>
        </BrowserRouter>
    );
}
