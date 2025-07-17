// src/services/CajaService.js
import axios from "axios";

const apiBaseURL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";

const apiClient = axios.create({
  baseURL: apiBaseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor para token
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem("token");
  if (token) {
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

export default {
  // Obtener cierre de un día específico
  getCierrePorFecha(fecha) {
    return apiClient.get("/cierrediario/por-fecha", {
      params: { fecha },
    });
  },

  // Obtener resumen del día (productos, servicios, pagos)
  getResumenDelDia(fecha) {
    return apiClient.get("/cierrediario/resumen", {
      params: { fecha },
    });
  },

  // Cerrar la caja
  cerrarCaja(payload) {
    // payload: { fecha: ISOString, observaciones: string, password: string }
    return apiClient.post("/cierrediario/cerrar", payload);
  },

  // Exportar cierre a PDF (opcional, si lo implementás después)
  exportarCierre(fecha) {
    return apiClient.get("/cierrediario/exportar", {
      params: { fecha },
      responseType: "blob", // Para descargar el archivo
    });
  },
};
