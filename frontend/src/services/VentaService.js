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

// Interceptor para agregar token
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem("token");
  if (token) {
    config.headers["Authorization"] = `Bearer ${token}`;
  }
  return config;
});

export default {
  // Obtener productos/servicios para venta con paginación y filtro por nombre (para autocompletado)
  getProductosServiciosVenta(page = 1, pageSize = 10, nombre = "") {
    console.log("Petición al backend con nombre:", nombre);
    return apiClient.get("/productosservicios/venta", {
      params: { page, pageSize, nombre },
    });
  },

  // Otros métodos que tengas para ventas, atención, etc, si los necesitas
  getVentas(
    page = 1,
    pageSize = 10,
    filtros = {},
    sortField = null,
    sortOrder = null
  ) {
    const params = {
      page,
      pageSize,
      ...filtros,
    };

    if (sortField) {
      params.ordenarPor = sortField;
      params.ordenDescendente = sortOrder === true; // o simplemente sortOrder
    }

    return apiClient.get("/detalleatencion/ventas", { params });
  },
  getVentaById(atencionId) {
    return apiClient.get(`/detalleatencion/ventas/${atencionId}`);
  },

  crearVenta(atencionData) {
    console.log(
      "Enviando payload al backend:",
      JSON.stringify(atencionData, null, 2)
    );
    return apiClient.post("/atencion", atencionData);
  },
  RegistrarPago(pagoData) {
    return apiClient.post("/pago", pagoData);
  },
  actualizarVenta(atencionId, datosActualizados) {
    console.log(
      "Payload para PUT /atencion/" + atencionId + ":",
      JSON.stringify(datosActualizados, null, 2)
    );
    return apiClient.put(`/atencion/${atencionId}`, datosActualizados);
  },
};
