import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import axios from "axios";
import authService from "./services/auth.service";
import Tooltip from "primevue/tooltip";
import "./style.css";

// Import PrimeVue
import PrimeVue from "primevue/config";

// Temas y estilos
import "primevue/resources/themes/md-dark-deeppurple/theme.css";
import "primevue/resources/primevue.min.css";
import "primeicons/primeicons.css";
import "primeflex/primeflex.css";
import ToastService from "primevue/toastservice";
import Toast from "primevue/toast";

//Cambio
const apiBaseURL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5042/api";

const apiClient = axios.create({
  baseURL: apiBaseURL,
});


//Original
// const apiClient = axios.create({
//   baseURL: "http://localhost:5042/api",
// });

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
app.use(PrimeVue);
app.directive('tooltip', Tooltip);

app.use(ToastService);
app.component("Toast", Toast);

// Montar la aplicación
app.mount("#app");
