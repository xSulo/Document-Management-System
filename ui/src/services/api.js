const API_BASE = "/api/documents";

async function http(url, options) {
    const res = await fetch(url, options);
    if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(`${res.status} ${res.statusText}${txt ? ` - ${txt}` : ""}`);
    }
    return res.status === 204 ? null : res.json();
}

export const DocumentsApi = {
    list: () => http(API_BASE),
    get: (id) => http(`${API_BASE}/${id}`),
    create: (payload) => http(API_BASE, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    }),
    update: (id, payload) => http(`${API_BASE}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    }),
    delete: (id) => http(`${API_BASE}/${id}`, { method: "DELETE" })
};
