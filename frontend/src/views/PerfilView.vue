<template>
  <div class="clientes-container">
    <Toast />

    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Mi Perfil</h4>
          <Button
            label="Editar"
            icon="pi pi-pencil"
            class="boton-editar"
            @click="abrirModal"
          />
        </div>
      </template>

      <template #content>
        <div v-if="usuarioActual" class="detalle-perfil">
          <div class="perfil-grid">
            <!-- Datos del usuario -->
            <div class="tabla-detalle-container">
              <table class="tabla-detalle">
                <tbody>
                  <tr>
                    <th>Nombre</th>
                    <td>{{ usuarioActual.nombre }}</td>
                  </tr>
                  <tr>
                    <th>Email</th>
                    <td>{{ usuarioActual.email }}</td>
                  </tr>
                  <tr>
                    <th>Teléfono</th>
                    <td>{{ usuarioActual.telefono || "No definido" }}</td>
                  </tr>
                  <tr>
                    <th>Rol</th>
                    <td>
                      {{
                        usuarioActual.rolNombre ||
                        usuarioActual.rol?.nombre ||
                        "—"
                      }}
                    </td>
                  </tr>
                  <tr>
                    <th>Estado</th>
                    <td>
                      <p
                        :class="[
                          'estado-texto',
                          usuarioActual.activo ? 'activo' : 'inactivo',
                        ]"
                      >
                        {{ usuarioActual.activo ? "Activo" : "Inactivo" }}
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <th>Fecha de Registro</th>
                    <td>{{ formatDate(usuarioActual.fechaRegistro) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Avatar -->
            <div class="avatar-container">
              <img
                :src="avatarUrl"
                :alt="'Avatar de ' + usuarioActual.nombre"
                class="avatar"
              />
            </div>
          </div>
        </div>

        <div v-else>Cargando...</div>
      </template>
    </Card>

    <!-- Modal de edición -->
    <Dialog
      v-model:visible="mostrarModal"
      header="Editar Perfil"
      :modal="true"
      :closeOnEscape="false"
      :closable="false"
      style="width: 450px"
    >
      <PerfilForm
        :usuario="usuarioTemp"
        @guardar="guardarPerfil"
        @cancelar="cerrarModal"
      />
    </Dialog>
  </div>
</template>

<script>
import UsuarioService from "../services/UsuarioService";
import Swal from "sweetalert2";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
import Tag from "primevue/tag";
import Card from "primevue/card";
import Toast from "primevue/toast";
import PerfilForm from "../components/PerfilForm.vue";

export default {
  name: "PerfilView",
  components: {
    Dialog,
    Button,
    Tag,
    Card,
    Toast,
    PerfilForm,
  },
  data() {
    return {
      usuarioActual: null,
      usuarioTemp: null,
      mostrarModal: false,
    };
  },
  computed: {
    avatarUrl() {
      if (!this.usuarioActual || !this.usuarioActual.avatar) {
        return "/avatars/no_avatar.jpg";
      }

      const apiBase =
        import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";
      const imageBase = apiBase.replace(/\/api\/?$/, ""); // quitar /api

      return `${imageBase}/${this.usuarioActual.avatar.replace(/^\//, "")}`;
    },
  },
  mounted() {
    this.obtenerDatosUsuario();
  },
  methods: {
    async obtenerDatosUsuario() {
      try {
        const res = await UsuarioService.getPerfil();
        this.usuarioActual = res.data.usuario;
      } catch (error) {
        console.error("Error al cargar los datos del perfil", error);
        Swal.fire("Error", "No se pudo cargar tu perfil.", "error");
        this.$router.push("/");
      }
    },
    abrirModal() {
      this.usuarioTemp = { ...this.usuarioActual };
      this.mostrarModal = true;
    },
    cerrarModal() {
      this.mostrarModal = false;
    },
    async guardarPerfil(formData) {
      try {
        await UsuarioService.actualizarPerfil(formData);

        const res = await UsuarioService.getPerfil();
        this.usuarioActual = res.data.usuario;

        // Actualiza el usuario guardado en sessionStorage
        sessionStorage.setItem("user", JSON.stringify(this.usuarioActual));

        Swal.fire({
          icon: "success",
          title: "Éxito",
          text: "Tu perfil ha sido actualizado correctamente.",
          timer: 1500,
          showConfirmButton: false,
          background: "#18181b",
          color: "#fff",
        });

        this.cerrarModal();
      } catch (error) {
        console.error("Error al guardar perfil:", error);
        const mensaje =
          error?.response?.data?.message || "No se pudo actualizar el perfil.";
        Swal.fire("Error", mensaje, "error");
      }
    },
    formatDate(date) {
      if (!date) return "";
      const options = { year: "numeric", month: "long", day: "numeric" };
      return new Date(date).toLocaleDateString("es-ES", options);
    },
  },
};
</script>

<style scoped>
.encabezado-acciones {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.detalle-perfil {
  width: 100%;
}

.perfil-grid {
  display: flex;
  gap: 2rem;
  flex-wrap: wrap; /* Para móviles, mostrar uno debajo del otro */
}

.tabla-detalle-container {
  background-color: #1a1a1a;
  padding: 1.5rem;
  border-radius: 10px;
  box-shadow: 0 3px 8px rgb(0 0 0 / 0.3);
  flex: 1;
  min-width: 300px;
}

.tabla-detalle {
  width: 100%;
  border-collapse: collapse;
  font-size: 1rem;
  color: #e0e0e0;
}

.tabla-detalle th,
.tabla-detalle td {
  padding: 0.75rem;
  border-bottom: 1px solid #333;
}

.tabla-detalle th {
  background-color: #1f1f1f;
  width: 30%;
  font-weight: bold;
  text-align: right;
  padding-right: 1rem;
}

.tabla-detalle td {
  text-align: left;
}

.avatar-container {
  display: flex;
  position: relative;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  flex: 1;
  min-width: 200px;
  padding: 2rem;
  background-color: #1f1f1f;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}
.avatar-wrapper {
  position: relative;
  display: inline-block;
}

.btn-eliminar-avatar {
  position: absolute;
  top: 6px;
  right: 6px;
  background: transparent;
  border: none;
  color: #fff; /* blanco para que contraste */
  font-size: 22px; /* icono más grande para visibilidad */
  cursor: pointer;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.3s ease, color 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  line-height: 1;
}

.avatar-wrapper:hover .btn-eliminar-avatar {
  opacity: 1;
  pointer-events: auto;
  color: #ff6b6b; /* cambio de color suave en hover para feedback */
}

.avatar {
  width: 240px;
  height: 240px;
  border-radius: 70%;
  object-fit: cover;
  border: 5px solid #4a90e2;
  background-color: #2d2d2d;
  box-shadow: 0 0 12px rgba(74, 144, 226, 0.5);
}

.avatar-nombre {
  margin-top: 1rem;
  font-size: 1.1rem;
  color: #ffffffcc;
  font-weight: 600;
  text-align: center;
}
.boton-editar {
  background-color: #f0ad4e; /* amarillo/dorado clásico */
  color: white;
  font-weight: normal;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.9rem;
  width: auto !important;
  height: auto !important;
  min-width: 120px !important;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.boton-editar:hover {
  background-color: #ec971f; /* un amarillo más oscuro para hover */
}
.estado-texto {
  font-weight: bold;
  padding: 0.3rem 0.75rem;
  border-radius: 16px;
  font-size: 0.85rem;
  display: inline-block;
  text-align: center;
}

.estado-texto.activo {
  background-color: #28a745;
  color: white;
}

.estado-texto.inactivo {
  background-color: #dc3545;
  color: white;
}
</style>
