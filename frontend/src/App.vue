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

  <div id="app">
    <AppTopbar class="topbar" v-if="isAuthenticated" />

    <div class="layout">
      <!-- Sidebar fijo a la izquierda -->
      <AppSidebar class="sidebar" :role="userRole" v-if="isAuthenticated" />

      <!-- Contenido principal a la derecha -->
      <main class="main-content">
        <router-view class="debug-border" />
      </main>
    </div>
  </div>
</template>

<style scoped>
html,
body {
  margin: 0;
  padding: 0;
  width: 100%;
  height: 100%;
}

#app {
  display: flex;
  flex-direction: column;
  height: 100vh; /* Ocupa todo el alto de la pantalla */
  width: 100vw; /* Ocupa todo el ancho de la pantalla */
  overflow: hidden;
}

/* Topbar fijo arriba */
.topbar {
  height: 60px;
  background-color: #27272a;
  flex-shrink: 0;
}

/* Layout horizontal: sidebar + contenido */
.layout {
  display: flex;
  flex: 1;
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
  margin-right: 2rem;
}
</style>
