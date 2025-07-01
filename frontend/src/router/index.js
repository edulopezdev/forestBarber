import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";

import ClientesView from "../views/ClientesView.vue";
import CrearCliente from "@/views/CrearCliente.vue";
import EditarCliente from "@/views/EditarCliente.vue";

import UsuariosView from "../views/UsuariosView.vue";

import ProductosView from "../views/ProductosView.vue";
import ServiciosView from "../views/ServiciosView.vue";

import VentasView from "../views/VentasView.vue";
import VentaForm from "../components/VentaForm.vue";
import VentaDetalle from "../components/VentaDetalle.vue";

import PerfilView from "../views/PerfilView.vue";

import LoginView from "../views/LoginView.vue";
import DashboardView from "../views/DashboardView.vue";
import authService from "../services/auth.service";

const routes = [
  { path: "/", redirect: "/login" },
  {
    path: "/login",
    name: "Login",
    component: LoginView,
  },
  {
    path: "/clientes",
    name: "Clientes",
    component: ClientesView,
    meta: { requiresAuth: true },
  },
  {
    path: "/clientes/nuevo",
    name: "CrearCliente",
    component: CrearCliente,
    meta: { requiresAuth: true },
  },
  {
    path: "/clientes/editar/:id",
    name: "EditarCliente",
    component: EditarCliente,
    meta: { requiresAuth: true },
  },
  {
    path: "/dashboard",
    name: "Dashboard",
    component: DashboardView,
    meta: { requiresAuth: true },
  },
  {
    path: "/home",
    name: "Home",
    component: HomeView,
    meta: { requiresAuth: true },
  },
  {
    path: "/usuarios",
    name: "Usuarios",
    component: UsuariosView,
    meta: { requiresAuth: true, requiredRole: "Administrador" },
  },
  {
    path: "/perfil",
    name: "Perfil",
    component: PerfilView,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/productos",
    name: "Productos",
    component: ProductosView,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/productos/nuevo",
    name: "NuevoProducto",
    component: () => import("../views/ProductosView.vue"),
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/productos/editar/:id",
    name: "EditarProducto",
    component: () => import("../views/ProductosView.vue"),
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/servicios",
    name: "Servicios",
    component: ServiciosView,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/servicios/nuevo",
    name: "NuevoServicio",
    component: () => import("../views/ServiciosView.vue"),
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/servicios/editar/:id",
    name: "EditarServicio",
    component: () => import("../views/ServiciosView.vue"),
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/ventas",
    name: "Ventas",
    component: VentasView,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/ventas/nuevo",
    name: "VentaNueva",
    component: VentaForm,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/ventas/:id/detalle",
    name: "VentaDetalle",
    component: VentaDetalle,
    props: true,
    meta: { requiresAuth: true, requiredRole: ["Administrador", "Barbero"] },
  },
  {
    path: "/:catchAll(.*)", // Redirige cualquier ruta no existente al login
    redirect: "/login",
  },
];

// Protección de rutas: redirigir a Login si no está autenticado
const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
  const isLoggedIn = authService.isAuthenticated();
  const requiredRole = to.meta.requiredRole;

  if (requiresAuth && !isLoggedIn) {
    return next("/login");
  }

  if (requiredRole) {
    const userRole = authService.getUserRole();
    const allowedRoles = Array.isArray(requiredRole)
      ? requiredRole
      : [requiredRole];
    if (!allowedRoles.includes(userRole)) {
      return next("/home");
    }
  }

  if (to.path === "/login" && isLoggedIn) {
    return next("/dashboard");
  }

  // Si no hubo redirección, continuar con la navegación normal
  next();
});

export default router;
