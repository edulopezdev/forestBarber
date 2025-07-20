import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import axios from "axios";
import authService from "./services/auth.service";
import Tooltip from "primevue/tooltip";
import "./style.css";
import TablaGlobal from "./components/TablaGlobal.vue";

// Import PrimeVue
import PrimeVue from "primevue/config";

// Temas y estilos
import "primevue/resources/themes/md-dark-deeppurple/theme.css";
import "primevue/resources/primevue.min.css";
import "primeicons/primeicons.css";
import "primeflex/primeflex.css";
import ToastService from "primevue/toastservice";
import Toast from "primevue/toast";

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

// Interceptor para incluir el token en cada petición
apiClient.interceptors.request.use(
  (config) => {
    const token = authService.getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

const app = createApp(App);

// Inyectar apiClient globalmente en la app
app.config.globalProperties.$api = apiClient;

// Usar router y PrimeVue
app.use(router);
app.use(PrimeVue, {
  ripple: true,
  inputStyle: 'filled',
  zIndex: {
    modal: 1100,        // z-index para modales
    overlay: 1000,      // z-index para overlays
    menu: 9999,         // z-index alto para menús desplegables
    tooltip: 1100       // z-index para tooltips
  },
  // Configuración para evitar desplazamientos
  appendTo: 'self'
});
app.directive('tooltip', Tooltip);

app.use(ToastService);
app.component("Toast", Toast);

// Montar la aplicación
app.mount("#app");

// Registrar el componente TablaGlobal globalmente
app.component("TablaGlobal", TablaGlobal);
