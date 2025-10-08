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

    // stays for now, might be relevant for meta data
    create: (payload) => http(API_BASE, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    }),

    upload: async (formData) => {
        const res = await fetch(API_BASE, {
            method: "POST",
            body: formData
        });
        if (!res.ok) throw new Error(await res.text());
        return res.json();
    },

    update: (id, payload) => http(`${API_BASE}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    }),
    delete: (id) => http(`${API_BASE}/${id}`, { method: "DELETE" })
};
