// importar axios, libreria que se usa para hacer peticiones HTTP (AJAX) desde el frontend
import axios from "axios";

// crear una instancia personalizada de axios con configuracion base
const apiClient = axios.create({
  baseURL: "http://localhost:5042/api", // URL base de nuestra API en ASP.NET Core
  headers: {
    "Content-Type": "application/json", //aca le indicamos q los datos se van a enviar en formato JSON
  },
});

// este es un interceptor q se ejecuta antes de cada peticion
// a su vez agregamos el token de autenticacion JWT al header si esta guardado en el navegador (sessionStorage)
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem("token");// recuperamos el token del sessionStorage
  if (token) {
    config.headers["Authorization"] = `Bearer ${token}`; // agregar el token en el encabezado Autorization
  }
  return config;
});

// exportamos un objeto que agrupa todas las funciones q usan AJAX para interactuar con la API
export default {
  // === FUNCIONES GENERALES ===

  // este metodo lo q hace es traer una lista paginada de usuarios con filtros opcionales
  getUsuarios(page = 1, pageSize = 10, filtros = {}) {
    const params = new URLSearchParams({
      page,
      pageSize,
      ...filtros, // si hay filtros como "nombre", se agragan a la URL
    });
    return apiClient.get(`/usuarios?${params.toString()}`);// Ej: /usuarios?page=1&pageSize=10&nombre=pepe
  },

  // este metodo trae un usuario por su id
  getUsuario(id) {
    return apiClient.get(`/usuarios/${id}`);
  },

  // este metodo crea un nuevo usuario (alta)
  crearUsuario(usuarioData) {
    const data = {
      nombre: usuarioData.nombre,
      email: usuarioData.email,
      telefono: usuarioData.telefono,
      rolId: usuarioData.rolId,// relaciona al usuario con un rol (Administrador, Barbero, Cliente)
      accedeAlSistema: usuarioData.accedeAlSistema ?? false, // si puede acceder al sistema
    };
    // solo se incluye el password si el usuario puede acceder al sistema
    if (usuarioData.accedeAlSistema && usuarioData.password) {
      data.password = usuarioData.password;
    }
    // llamada a la API
    return apiClient.post("/usuarios", data);
  },

  // este metodo actualiza un usuario
  actualizarUsuario(id, usuarioData) {
    const data = {
      nombre: usuarioData.nombre,
      email: usuarioData.email,
      telefono: usuarioData.telefono,
      rolId: usuarioData.rolId,
      accedeAlSistema: usuarioData.accedeAlSistema ?? false,
    };

    return apiClient.put(`/usuarios/${id}`, data);
  },

  // este metodo elimina un usuario (baja logica)
  eliminarUsuario(id) {
    return apiClient.delete(`/usuarios/${id}`);
  },

  // este metodo cambia el estado de un usuario
  cambiarEstado(id, activo) {
    return apiClient.patch(`/usuarios/${id}/estado`, { activo });
  },

  // === FUNCIONES PARA CLIENTES (rolId = 3) ===

  // este metodo trae una lista paginada de clientes (usuarios con rolId = 3)
  getClientes(page = 1, pageSize = 10, filtros = {}) {
    const params = new URLSearchParams({
      page,
      pageSize,
      ...filtros,// si hay filtros como "nombre", se agragan a la URL
    });

    return apiClient.get(`/usuarios/clientes?${params.toString()}`);// Ej: /usuarios/clientes?page=1&pageSize=10&nombre=pepe
  },

  // este metodo trae un cliente por su id
  getCliente(id) {
    return apiClient.get(`/usuarios/${id}`);
  },

  // este metodo crea un nuevo cliente (sin acceso al sistema)
  crearCliente(clienteData) {
    const data = {
      nombre: clienteData.nombre,
      email: clienteData.email,
      telefono: clienteData.telefono,
      rolId: 3,
      accedeAlSistema: false, //no puede acceder al sistema
    };

    return apiClient.post("/usuarios", data);
  },

  // este metodo actualiza un cliente
  actualizarUsuario(id, usuarioData) {
    const data = {
      nombre: usuarioData.nombre,
      email: usuarioData.email,
      telefono: usuarioData.telefono,
      rolId: usuarioData.rolId,
      accedeAlSistema: usuarioData.accedeAlSistema ?? false,
      activo: usuarioData.activo,
    };

    if (usuarioData.password && usuarioData.password.trim() !== "") {
      data.password = usuarioData.password;// solo se incluye el password si el usuario puede acceder al sistema
    }

    return apiClient.put(`/usuarios/${id}`, data);
  },

  // === FUNCIONES PARA ADMINISTRADORES (rolId = 1) ===

  // trae el perfil del usuario actual (por su token)
  getPerfil() {
    return apiClient.get("/usuarios/perfil");
  },

  // actualiza el perfil del usuario logueado
  actualizarPerfil(usuarioData) {
    return apiClient.put("/usuarios/perfil", usuarioData, {
      headers: {
        "Content-Type": "multipart/form-data",// para enviar archivos usamos multipart data
      },
    });
  },
};
