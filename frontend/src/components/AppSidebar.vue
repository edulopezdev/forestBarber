<template>
  <div class="sidebar">
    <!-- Nombre o logo de la app -->
    <img
      src="../assets/images/logo.png"
      alt="Logo Barbería López"
      class="app-logo"
    />
    <ul class="menu-list">
      <li v-for="(item, i) in filteredMenuItems" :key="i">
        <router-link :to="item.route" class="menu-item" active-class="active">
          <i :class="['menu-icon', item.icon]"></i>
          <span class="menu-label">{{ item.label }}</span>
        </router-link>
      </li>
    </ul>
  </div>
</template>

<script>
import authService from "../services/auth.service";

export default {
  props: {
    role: { type: String, required: true },
  },
  name: "AppSidebar",
  data() {
    return {
      isAuthenticated: false,
      userRole: null,
      menuItems: [
        {
          label: "Inicio",
          route: "/dashboard",
          icon: "pi pi-home",
          rolesAllowed: ["Administrador", "Barbero"],
        },
        {
          label: "Clientes",
          route: "/clientes",
          icon: "pi pi-users",
          rolesAllowed: ["Administrador", "Barbero"],
        },
        // {
        //   label: "Turnos",
        //   route: "/turnos",
        //   icon: "pi pi-calendar",
        //   rolesAllowed: ["Administrador", "Barbero"],
        // },
        {
          label: "Productos",
          route: "/productos",
          icon: "pi pi-box",
          rolesAllowed: ["Administrador", "Barbero"],
        },
        {
          label: "Servicios",
          route: "/servicios",
          icon: "pi pi-wrench",
          rolesAllowed: ["Administrador", "Barbero"],
        },
        {
          label: "Ventas",
          route: "/ventas",
          icon: "pi pi-credit-card",
          rolesAllowed: ["Administrador", "Barbero"],
        },
        {
          label: "Reportes",
          route: "/reportes",
          icon: "pi pi-credit-card",
          rolesAllowed: ["Administrador"],
        },
        {
          label: "Usuarios",
          route: "/usuarios",
          icon: "pi pi-user",
          rolesAllowed: ["Administrador"],
        },
        {
          label: "Perfil",
          route: "/perfil",
          icon: "pi pi-user-edit",
          rolesAllowed: ["Administrador", "Barbero"],
        },
      ],
    };
  },
  watch: {
    $route: {
      immediate: true,
      handler() {
        this.isAuthenticated = authService.isAuthenticated();
        const user = authService.getUser();
        this.userRole = user?.rol || null;
      },
    },
  },
  computed: {
    filteredMenuItems() {
      const filtrados = this.menuItems.filter(
        (item) => item.rolesAllowed && item.rolesAllowed.includes(this.role)
      );
      return filtrados;
    },
  },
};
</script>

<style>
:root {
  --sidebar-margin: 1rem;
  --topbar-height: 4rem;
  --color-active-bg: #28a745;
  --color-active-text: #ffffff;
}
</style>

<style scoped>
.sidebar {
  position: fixed;
  top: calc(var(--topbar-height) + var(--sidebar-margin));
  left: var(--sidebar-margin);
  z-index: 100;
  width: 250px;
  height: calc(100vh - var(--topbar-height) - calc(var(--sidebar-margin) * 2));
  background-color: var(--surface-overlay);
  border-radius: 0.5rem;
  box-shadow: var(--shadow-2);
  padding: 1rem;
  overflow-y: auto;
  transition: box-shadow 0.3s ease;
}

.app-logo {
  height: 60px;
  object-fit: contain;
  margin-bottom: 1.5rem;
  display: block;
  margin-left: auto;
  margin-right: auto;
}

.menu-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.menu-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1.5rem;
  color: var(--text-color);
  text-decoration: none;
  border-radius: 0.5rem;
  transition: background-color 0.3s ease, color 0.3s ease;
}

.menu-item:hover {
  background-color: var(--surface-hover);
  color: var(--text-color);
}

.menu-item.active {
  color: var(--color-active-text);
  font-weight: 600;
}

.menu-item.active .menu-icon {
  color: var(--color-active-text);
}

.menu-icon {
  margin-right: 0.75rem;
  font-size: 1.2rem;
  color: var(--text-color);
  transition: color 0.3s ease;
}

.menu-label {
  font-size: 1rem;
  font-weight: 500;
}
</style>
