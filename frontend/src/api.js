const API_URL =
   import.meta.env.VITE_API_URL ?? "https://localhost:7196"; 

export function getToken() {
   return sessionStorage.getItem("token");
}

let onUnauthorized = () => {};
export function setUnauthorizedHandler(fn) {
   onUnauthorized = fn;
}

export async function api(path, options = {}) {
   
   const token = getToken();
   const res = await fetch(`${API_URL}${path}`, {
      ...options,
      headers: {
         "Content-Type": "application/json",
         ...(token ? {
            Authorization: `Bearer ${token}`
         } : {}),
         ...options.headers,
      },
   });

   const data = res.status === 204 ? null : await res.json().catch(() => null);

   if (!res.ok) {
      const message = data ?.message ?? data ?.title ?? "Something went wrong.";
      const error = new Error(message);
      error.status = res.status;

      if (res.status === 401 && token) {
         onUnauthorized(); // an expired or invalid token: force logout
      }


      throw error;
   }
   return data;
}