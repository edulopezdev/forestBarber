<script>
import axios from "axios";
import Swal from "sweetalert2";

export default {
  data() {
    return {
      usuario: {
        email: "",
        password: "",
      },
    };
  },
  methods: {
    mostrarToast({ icon = "info", title = "", timer = 3000 }) {
  const iconColors = {
    success: "#28a745",
    error: "#dc3545",
    warning: "#ffc107",
    info: "#17a2b8",
  };

  Swal.fire({
    toast: true,
    position: "top-end",
    showConfirmButton: false,
    timer,
    timerProgressBar: true,
    icon,
    title,
    background: "#18181b",
    color: "#ffffff",
    iconColor: iconColors[icon] || "#ffffff",
  });
},


    iniciarSesion() {
      if (!this.usuario.email || !this.usuario.password) {
        this.mostrarToast({
          icon: "warning",
          title: "Por favor ingresa email y contraseña",
        });
        return;
      }

      const apiBaseURL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000/api";
      const url = `${apiBaseURL}/auth/login`;

      Swal.fire({
        title: "Iniciando sesión...",
        allowOutsideClick: false,
        background: "#18181b",
        color: "#fff",
        didOpen: () => {
          Swal.showLoading();

          setTimeout(() => {
            axios
              .post(url, this.usuario)
              .then((res) => {
                Swal.close();
                sessionStorage.setItem("token", res.data.token);
                sessionStorage.setItem(
                  "user",
                  JSON.stringify(res.data.usuario)
                );
                this.$nextTick(() => {
                  this.$router.push("/dashboard");
                });
              })
              .catch((err) => {
                Swal.close();
                if (!err.response) {
                  this.mostrarToast({
                    icon: "error",
                    title: "No se pudo conectar con el servidor",
                  });
                } else {
                  const mensaje =
                    err.response.data?.message || "Error al iniciar sesión.";
                  this.mostrarToast({
                    icon: "error",
                    title: mensaje,
                  });
                }
                console.error("Detalles del error:", err);
              });
          }, 500); // medio segundo de delay intencional
        },
      });
    },
  },
};
</script>

<template>
  <div class="login-container">
    <Card>
      <template #title>Iniciar Sesión</template>
      <template #content>
        <div class="p-fluid">
          <div class="p-field">
            <label for="email">Email</label>
            <InputText
              id="email"
              v-model="usuario.email"
              type="email"
              placeholder="tu@correo.com"
              @keyup.enter="iniciarSesion"
            />
          </div>
          <div class="p-field">
            <label for="password">Contraseña</label>
            <Password
              id="password"
              v-model="usuario.password"
              placeholder="*********"
              toggleMask
              :feedback="false"
              @keyup.enter="iniciarSesion"
            />
          </div>
          <Button
            label="Entrar"
            icon="pi pi-sign-in"
            @click="iniciarSesion"
            block
            severity="success"
          />
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.login-container {
  max-width: 450px;
  margin: 5rem auto;
  padding: 2rem;
  transform: translateX(-120px);
}

/* Estilo del Card */
:deep(.p-card) {
  background-color: #18181b;
  color: #ffffff;
  border-radius: 1rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  border: none;
}

/* Títulos y contenido del card */
:deep(.p-card-title),
:deep(.p-card-content),
:deep(.p-card-body) {
  color: #ffffff;
}

/* Campo y etiquetas */
.p-fluid .p-field {
  margin-bottom: 1.5rem;
}

.p-field label {
  font-weight: bold;
  color: #e0e0e0;
}

/* Inputs */
.p-inputtext,
.p-password input {
  border-radius: 5px;
  padding: 0.8rem;
  background-color: #2a2a2e;
  border: 1px solid #3a3a3e;
  color: #ffffff;
}

.p-inputtext::placeholder,
.p-password input::placeholder {
  color: #cccccc;
}

.p-inputtext:focus,
.p-password input:focus {
  border-color: #28a745;
  box-shadow: 0 0 5px rgba(40, 167, 69, 0.4);
}

/* Botón */
.p-button {
  border-radius: 5px;
  font-size: 1rem;
}

.p-button:focus {
  outline: none;
}

.p-button-success {
  background-color: #28a745;
  border: 1px solid #28a745;
  color: #fff;
}

.p-button-success:hover {
  background-color: #218838;
}

.p-button-success:active {
  background-color: #1e7e34;
}
/* Fondo y texto para input contraseña */
.p-password input {
  background-color: #2a2a2e !important;
  color: #ffffff !important;
  border: 1px solid #3a3a3e !important;
}

/* Placeholder color */
.p-password input::placeholder {
  color: #cccccc !important;
}
</style>
