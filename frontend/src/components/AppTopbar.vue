<template>
  <div class="app-topbar">
    <Toolbar class="toolbar">
      <template #start>
        <div class="topbar-left">
          <SessionTimer v-if="usuarioLogueado" />
        </div>
      </template>

      <template #end>
        <div v-if="usuarioLogueado" class="usuario-info">
          <div class="usuario-datos">
            <img 
              :src="avatarUrl" 
              :alt="`Avatar de ${usuarioLogueado.nombre || usuarioLogueado.email}`" 
              class="avatar"
              @error="handleImageError"
            />
            <span class="nombre-usuario">{{ usuarioLogueado.email }}</span>
          </div>
          <Button
            icon="pi pi-sign-out"
            class="logout-btn"
            rounded
            severity="danger"
            text
            @click="logout"
            v-tooltip="'Cerrar sesión'"
          />
        </div>
      </template>
    </Toolbar>
  </div>
</template>

<script>


import Toolbar from "primevue/toolbar";
import Button from "primevue/button";
import authService from "../services/auth.service";
import { confirmDialog } from "../utils/confirmDialog";
import Swal from "sweetalert2";
import SessionTimer from "./SessionTimer.vue";

export default {
  name: "AppTopbar",
  components: {
    Toolbar,
    Button,
  },
  data() {
    return {
      defaultAvatar: '/img/default-avatar.svg', // Avatar por defecto más profesional
      imageError: false
    };
  },
  computed: {
    usuarioLogueado() {
      const usuario = authService.getUser();
      if (!usuario) return null;

      return {
        ...usuario,
      };
    },
    avatarUrl() {
      if (this.imageError) {
        return this.defaultAvatar;
      }

      if (!this.usuarioLogueado) return this.defaultAvatar;

      const baseUrl = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";
      const imageBaseUrl = baseUrl.replace(/\/api\/?$/, ""); // quitar /api

      // Si el usuario no tiene avatar o está vacío, usar imagen por defecto
      if (!this.usuarioLogueado.avatar || !this.usuarioLogueado.avatar.trim()) {
        return this.defaultAvatar;
      }

      // Construir URL del avatar del backend
      return `${imageBaseUrl}/${this.usuarioLogueado.avatar.replace(/^\//, "")}`;
    },
  },
  watch: {
    // Opcional: escuchar cambios en usuarioLogueado
    usuarioLogueado(newVal) {
      console.log("Usuario logueado actualizado:", newVal);
    },
  },
  methods: {
    handleImageError() {
      console.log('Error al cargar imagen de avatar, usando imagen por defecto');
      this.imageError = true;
    },
    async logout() {
      const result = await confirmDialog({
        title: "Cerrar sesión",
        message: "¿Estás seguro de que querés salir?",
      });

      if (result.isConfirmed) {
        authService.logout();
        this.$router.push("/login");

        Swal.fire({
          title: "Sesión cerrada",
          text: "Has salido correctamente.",
          icon: "success",
          timer: 2000,
          showConfirmButton: false,
          background: "#18181b",
          color: "#fff",
        });
      }
    },
  },
};
</script>

<style scoped>
.app-topbar {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  z-index: 1000;
  background-color: #18181b;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.06);
  padding: 0.5rem 1rem;
}

.toolbar {
  background-color: transparent;
  padding: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.usuario-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.usuario-datos {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.nombre-usuario {
  font-weight: 500;
  color: #fff;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #4a90e2;
  background-color: #f8f9fa;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.avatar:hover {
  transform: scale(1.05);
  border-color: #357abd;
  box-shadow: 0 4px 8px rgba(74, 144, 226, 0.3);
}

.logout-btn {
  width: 36px;
  height: 36px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  color: #ffffff;
  background-color: transparent;
  border: none;
  transition: background-color 0.3s ease;
}

.logout-btn:hover {
  background-color: #5d5d5d;
}
</style>
