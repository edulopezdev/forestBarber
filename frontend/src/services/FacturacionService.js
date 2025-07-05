import axios from "axios";

//Original
// const apiClient = axios.create({
//   baseURL: "http://localhost:5000/api",
//   headers: {
//     "Content-Type": "application/json",
//   },
// });

//Cambio
const apiBaseURL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";

const apiClient = axios.create({
  baseURL: apiBaseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Interceptor para agregar token si existe
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem("token");
  if (token) {
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

export default {
  getFacturacionPorFecha(fecha) {
    const formattedDate =
      fecha instanceof Date ? fecha.toISOString().split("T")[0] : fecha;
    return apiClient.get(`/Pago/facturacion?fecha=${formattedDate}`);
  },
  getFacturacionPorMes(anio, mes) {
    return apiClient.get(`/Pago/facturacion-mes`, {
      params: { anio, mes },
    });
  },
};
