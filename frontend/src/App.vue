<script>
import AppSidebar from "./components/AppSidebar.vue";
import AppTopbar from "./components/AppTopbar.vue";
import authService from "./services/auth.service";

export default {
  components: {
    AppSidebar,
    AppTopbar,
  },
  data() {
    return {
      isAuthenticated: false,
      userRole: null,
    };
  },
  watch: {
    $route: {
      immediate: true,
      handler() {
        this.isAuthenticated = authService.isAuthenticated();
        this.userRole = authService.getUserRole();
      },
    },
  },
  mounted() {
    this.isAuthenticated = authService.isAuthenticated();
    this.userRole = authService.getUserRole();
  },
};
</script>

<template>
  <Toast />

  <AppTopbar class="topbar" v-if="isAuthenticated" />

  <div class="layout">
    <!-- Sidebar fijo a la izquierda -->
    <AppSidebar class="sidebar" :role="userRole" v-if="isAuthenticated" />

    <!-- Contenido principal a la derecha -->
    <main class="main-content">
      <router-view class="debug-border" />
    </main>
  </div>
</template>

<style scoped>
html,
body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
  overflow: auto;
}

/* Topbar fijo arriba */
.topbar {
  height: 60px;
  background-color: #27272a;
  flex-shrink: 0;
}

.layout {
  display: flex;
  height: calc(100vh - 60px - 0px);
  margin-top: 28px;
  width: calc(100% - -28px); /* Evita que se pase del viewport */
  overflow: hidden;
}

/* Sidebar a la izquierda */
.sidebar {
  width: 250px;
  background-color: #1f1f1f;
  flex-shrink: 0;
}

/* Contenido principal (a la derecha del sidebar) */
.main-content {
  flex-grow: 1;
  overflow-y: auto;
  padding: 1.5rem;
  margin-left: 230px; /* Respeta el espacio del sidebar */
  padding-right: 1.5rem;
  background-color: transparent;

  /* Estilo del scrollbar para Firefox */
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.2) transparent;
}

/* Scrollbar estilizado y discreto para WebKit (Chrome, Edge, Safari) */
.main-content::-webkit-scrollbar {
  width: 8px;
}

.main-content::-webkit-scrollbar-track {
  background: transparent;
}

.main-content::-webkit-scrollbar-thumb {
  background-color: rgba(255, 255, 255, 0.2);
  border-radius: 4px;
  transition: background-color 0.3s;
}

.main-content::-webkit-scrollbar-thumb:hover {
  background-color: rgba(255, 255, 255, 0.35);
}
</style>
