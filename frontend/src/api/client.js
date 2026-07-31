import axios from "axios";

// Cambia la baseURL según dónde corra tu backend:
//   - Docker (docker compose up):   http://localhost:8080
//   - Local (dotnet run):           http://localhost:5133
export const api = axios.create({ baseURL: "http://localhost:8080" });

// Interceptor de request: agrega el token JWT a cada petición
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Interceptor de response: si el token expira (401), saca al usuario al login
api.interceptors.response.use(
  (r) => r,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(err);
  }
);
