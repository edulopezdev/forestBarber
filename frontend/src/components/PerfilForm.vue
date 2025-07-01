<template>
  <div class="formulario-cliente usuario-form">
    <h3>Editar Perfil</h3>

    <!-- Nombre -->
    <div class="campo" :class="{ error: errores.nombre }">
      <label for="nombre">Nombre <span class="obligatorio">*</span></label>
      <InputText id="nombre" v-model="form.nombre" autofocus />
      <div v-if="errores.nombre" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.nombre }}
      </div>
    </div>

    <!-- Email -->
    <div class="campo" :class="{ error: errores.email }">
      <label for="email">Email <span class="obligatorio">*</span></label>
      <InputText id="email" v-model="form.email" type="email" />
      <div v-if="errores.email" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.email }}
      </div>
    </div>

    <!-- Teléfono -->
    <div class="campo" :class="{ error: errores.telefono }">
      <label for="telefono">Teléfono <span class="obligatorio">*</span></label>
      <InputText
        id="telefono"
        v-model="form.telefono"
        @keypress="soloNumeros"
        @input="form.telefono = form.telefono.replace(/\D/g, '')"
        placeholder="Solo números"
      />
      <div v-if="errores.telefono" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.telefono }}
      </div>
    </div>

    <!-- Rol actual (solo lectura) -->
    <div class="campo">
      <label for="rolActual">Rol Actual</label>
      <InputText
        id="rolActual"
        :value="form.rolNombre"
        readonly
        style="background-color: #2d2d2d; color: #ffffff"
      />
    </div>

    <!-- Avatar -->
    <div class="campo">
      <label>Avatar</label>
      <div class="avatar-preview">
        <img :src="avatarUrl" alt="Avatar" />
      </div>
      <div class="avatar-actions">
        <FileUpload
          mode="basic"
          name="avatar"
          accept="image/*"
          chooseLabel=""
          chooseIcon="pi pi-pencil"
          @select="onAvatarSeleccionado"
          :auto="true"
          customUpload
          class="boton-avatar fileupload-icon"
        />
        <Button
          icon="pi pi-trash"
          class="boton-avatar boton-avatar-eliminar"
          @click="eliminarAvatar"
          v-if="form.avatar || previewAvatar"
        />
      </div>
    </div>

    <!-- Contraseña -->
    <div class="campo" :class="{ error: errores.password }">
      <label for="password">
        Nueva Contraseña
        <small style="font-weight: normal; font-size: 0.8rem; color: #666">
          (Dejar vacío para no cambiar)
        </small>
      </label>
      <InputText
        id="password"
        v-model="form.password"
        type="password"
        autocomplete="new-password"
      />
      <div v-if="errores.password" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.password }}
      </div>
    </div>

    <!-- Botones -->
    <div class="acciones-formulario">
      <div class="contenedor-boton">
        <Button
          class="btn-guardar"
          label="Guardar"
          icon="pi pi-check"
          @click="onGuardar"
        />
      </div>
      <button class="btn-cerrar" @click="$emit('cancelar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import InputText from "primevue/inputtext";
import Button from "primevue/button";
import FileUpload from "primevue/fileupload";

export default {
  name: "PerfilForm",
  components: { InputText, Button, FileUpload },
  props: {
    usuario: Object,
  },
  data() {
    return {
      form: {
        id: null,
        nombre: "",
        email: "",
        telefono: "",
        rolNombre: "",
        avatar: "",
        password: "",
        nuevoAvatar: null,
        eliminarAvatarFlag: false,
      },
      previewAvatar: null,
      errores: {
        nombre: null,
        email: null,
        telefono: null,
        password: null,
      },
    };
  },
  mounted() {
    if (this.usuario) {
      this.form = {
        id: this.usuario.id,
        nombre: this.usuario.nombre || "",
        email: this.usuario.email || "",
        telefono: this.usuario.telefono || "",
        rolNombre:
          this.usuario.rolNombre || this.usuario.rol?.nombre || "No definido",
        avatar: this.usuario.avatar || "",
        password: "",
      };
    }
  },
  methods: {
    soloNumeros(event) {
      const charCode = event.charCode ? event.charCode : event.keyCode;
      if (charCode < 48 || charCode > 57) {
        event.preventDefault();
      }
    },
    onAvatarSeleccionado(event) {
      const file = event.files[0];
      if (file) {
        this.form.nuevoAvatar = file;
        this.previewAvatar = URL.createObjectURL(file);
      }
    },
    eliminarAvatar() {
      this.form.avatar = null;
      this.form.nuevoAvatar = null;
      this.previewAvatar = null;
      this.eliminarAvatarFlag = true;
    },
    validarFormulario() {
      this.errores = {
        nombre: null,
        email: null,
        telefono: null,
        password: null,
      };
      let valido = true;
      const camposConError = [];

      if (!this.form.nombre.trim()) {
        this.errores.nombre = "El nombre es obligatorio.";
        camposConError.push("nombre");
        valido = false;
      }

      if (!this.form.email.trim()) {
        this.errores.email = "El email es obligatorio.";
        camposConError.push("email");
        valido = false;
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.form.email)) {
        this.errores.email = "El email no es válido.";
        camposConError.push("email");
        valido = false;
      }

      if (!this.form.telefono.trim()) {
        this.errores.telefono = "El teléfono es obligatorio.";
        camposConError.push("telefono");
        valido = false;
      } else if (/\D/.test(this.form.telefono)) {
        this.errores.telefono = "El teléfono debe contener solo números.";
        camposConError.push("telefono");
        valido = false;
      }

      if (camposConError.length > 0) {
        this.$nextTick(() => {
          const campo = document.getElementById(camposConError[0]);
          if (campo) {
            campo.scrollIntoView({ behavior: "smooth", block: "center" });
            campo.focus();
          }
        });
      }

      return valido;
    },
    onGuardar() {
      if (!this.validarFormulario()) return;

      const formData = new FormData();
      formData.append("Nombre", this.form.nombre);
      formData.append("Email", this.form.email);
      formData.append("Telefono", this.form.telefono);
      if (this.form.password) {
        formData.append("Password", this.form.password);
      }
      if (this.form.nuevoAvatar) {
        formData.append("Avatar", this.form.nuevoAvatar);
      }
      if (this.eliminarAvatarFlag) {
        formData.append("EliminarAvatar", "true");
      }

      this.$emit("guardar", formData);
    },
  },
  computed: {
    avatarUrl() {
      if (this.previewAvatar) return this.previewAvatar;
      if (this.form.avatar) {
        const baseUrl =
          import.meta.env.VITE_API_BASE_URL || "http://localhost:5042";
        return this.form.avatar.startsWith("http")
          ? this.form.avatar
          : `${baseUrl}${this.form.avatar}`;
      }
      return "/img/avatar-placeholder.png";
    },
  },
};
</script>

<style scoped>
.formulario-cliente {
  max-width: 400px;
  margin: 0 auto;
  padding: 1rem;
}

.campo {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
}

.campo.error label {
  color: #e74c3c;
  font-weight: 700;
}

label {
  font-weight: 600;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.obligatorio {
  color: #e74c3c;
  font-size: 1.2rem;
  line-height: 1;
  font-weight: 900;
  user-select: none;
  animation: pulse 1.5s infinite alternate ease-in-out;
  margin-left: 0.1rem;
}

@keyframes pulse {
  0% {
    opacity: 1;
    transform: scale(1);
  }
  100% {
    opacity: 0.6;
    transform: scale(1.2);
  }
}

.error-msg {
  margin-top: 0.35rem;
  background-color: #f9d6d5;
  border: 1.5px solid #e74c3c;
  color: #a94442;
  padding: 0.35rem 0.6rem;
  border-radius: 6px;
  font-size: 0.875rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  user-select: none;
}

.error-msg i {
  font-size: 1rem;
}

/* Botones */
.acciones-formulario {
  display: flex;
  justify-content: space-between;
  margin-top: 1.5rem;
}

.p-button {
  background-color: #4a90e2;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 0.5rem 1.2rem;
  transition: background-color 0.3s ease, box-shadow 0.3s ease;
}

.p-button:hover {
  background-color: #357abd;
  box-shadow: 0 0 8px #357abd88;
}

.p-button.p-button-danger {
  background-color: #e74c3c;
  color: white;
  border-radius: 6px;
  padding: 0.5rem 1.2rem;
  transition: background-color 0.3s ease, box-shadow 0.3s ease;
}

.p-button.p-button-danger:hover {
  background-color: #c0392b;
  box-shadow: 0 0 8px #c0392b88;
}

.toolbar-buttons {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-top: 0.5rem;
}

.icon-button {
  width: 2.2rem;
  height: 2.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
  font-size: 1rem;
}

.avatar-actions {
  display: flex;
  gap: 0.3rem;
  margin-top: 0.5rem;
  justify-content: flex-start;
  align-items: center;
}

.boton-avatar {
  background-color: transparent !important;
  border: none !important;
  width: 30px;
  height: 30px;
  min-width: 30px;
  padding: 0.25rem !important;
  font-size: 0.9rem;
  color: #ffffff !important;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: none !important;
  border-radius: 6px !important;
  transition: background-color 0.2s ease;
}

.boton-avatar:hover {
  background-color: #2e2e2e !important;
}

.boton-avatar-eliminar:hover {
  background-color: #c0392b !important;
}

/* FileUpload button styling */
/* Oculta el texto dentro del botón choose y centra el ícono */
:deep(.p-fileupload-choose .p-button-label) {
  display: none !important;
}

/* Asegura que el icono esté centrado y con estilo igual al botón eliminar */
:deep(.p-fileupload-choose) {
  background-color: transparent !important;
  border: none !important;
  box-shadow: none !important;
  padding: 0.25rem !important;
  width: 30px;
  height: 30px;
  min-width: 30px;
  border-radius: 6px !important;
  display: flex !important;
  align-items: center;
  justify-content: center;
  color: #ffffff !important;
  font-size: 1rem;
  transition: background-color 0.2s ease;
  cursor: pointer;
}

:deep(.p-fileupload-choose:hover) {
  background-color: #2e2e2e !important;
}
</style>
