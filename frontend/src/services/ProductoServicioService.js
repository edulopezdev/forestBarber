import axios from "axios";

//Original
// const apiClient = axios.create({
//   baseURL: "http://localhost:5042/api",
// });

//Camnio
const apiBaseURL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
const apiClient = axios.create({
  baseURL: apiBaseURL,
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
  // ==============================
  // === PRODUCTOS (EsAlmacenable = true)
  // ==============================
  getProductos(page = 1, pageSize = 10, filtros = {}) {
    console.log("[ProductoServicioService] getProductos", {
      page,
      pageSize,
      filtros,
    });
    const params = new URLSearchParams();
    params.append("page", page);
    params.append("pageSize", pageSize);
    Object.entries(filtros).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== "") {
        params.append(key, value);
      }
    });
    return apiClient.get(
      `/productosservicios/almacenables?${params.toString()}`
    );
  },
  getProductosYServiciosParaVenta(page = 1, pageSize = 10, filtros = {}) {
    const params = new URLSearchParams();
    params.append("page", page);
    params.append("pageSize", pageSize);
    Object.entries(filtros).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== "") {
        params.append(key, value);
      }
    });

    return apiClient.get(`/productosservicios/venta?${params.toString()}`);
  },

  getProducto(id) {
    return apiClient.get(`/productosservicios/${id}`);
  },

  crearProducto(formData) {
    return apiClient.post("/productosservicios", formData);
  },

  actualizarProducto(id, formData) {
    return apiClient.put(`/productosservicios/${id}`, formData);
  },

  eliminarProducto(id) {
    return apiClient.delete(`/productosservicios/${id}`);
  },

  // ==============================
  // === SERVICIOS (EsAlmacenable = false)
  // ==============================
  getServicios(page = 1, pageSize = 10, filtros = {}) {
    console.log("[ProductoServicioService] getServicios", {
      page,
      pageSize,
      filtros,
    });
    const params = new URLSearchParams();
    params.append("page", page);
    params.append("pageSize", pageSize);
    Object.entries(filtros).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== "") {
        params.append(key, value);
      }
    });
    return apiClient.get(
      `/productosservicios/noAlmacenables?${params.toString()}`
    );
  },

  getServicio(id) {
    return apiClient.get(`/productosservicios/${id}`);
  },

  crearServicio(formData) {
    return apiClient.post("/productosservicios", formData);
  },

  actualizarServicio(id, formData) {
    return apiClient.put(`/productosservicios/${id}`, formData);
  },

  eliminarServicio(id) {
    return apiClient.delete(`/productosservicios/${id}`);
  },

  // ==============================
  // === IMÁGENES (comunes a ambos)
  // ==============================
  eliminarImagen(idImagen) {
    return apiClient.delete(`/productosservicios/imagen/${idImagen}`);
  },

  obtenerImagen(idProductoServicio) {
    return apiClient.get(`/productosservicios/${idProductoServicio}/imagen`, {
      responseType: "blob",
    });
  },
};
