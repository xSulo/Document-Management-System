export default function Toast({ message, type = "success" }) {
    if (!message) return null;

    const base =
        "fixed left-1/2 transform -translate-x-1/2 top-20 z-50 px-6 py-3 rounded-lg shadow-lg text-sm font-semibold transition-all duration-500 w-fit";

    const styles =
        type === "success"
            ? "bg-green-500 text-white"
            : type === "error"
                ? "bg-red-500 text-white"
                : "bg-gray-700 text-white";

    return (
        <div className={`${base} ${styles} animate-fade-in`}>
            {message}
        </div>
    );
}
