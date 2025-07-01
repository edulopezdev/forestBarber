<template>
  <div class="formulario-cliente usuario-form">
    <h3>{{ form.id ? "Editar Usuario" : "Nuevo Usuario" }}</h3>

    <div class="campo" :class="{ error: errores.nombre }">
      <label for="nombre">Nombre <span class="obligatorio">*</span></label>
      <InputText id="nombre" v-model="form.nombre" autofocus />
      <div v-if="errores.nombre" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.nombre }}
      </div>
    </div>

    <div class="campo" :class="{ error: errores.email }">
      <label for="email">Email <span class="obligatorio">*</span></label>
      <InputText id="email" v-model="form.email" type="email" />
      <div v-if="errores.email" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.email }}
      </div>
    </div>

    <div class="campo" :class="{ error: errores.telefono }">
      <label for="telefono">Teléfono</label>
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
    <div class="campo" v-if="esPerfil">
      <label for="rolActual">Rol Actual</label>
      <InputText
        id="rolActual"
        :value="form.rolNombre"
        readonly
        style="background-color: #2d2d2d; color: #ffffff"
      />
    </div>
    <div class="campo" :class="{ error: errores.rolId }" v-if="!esPerfil">
      <label for="rol">Rol <span class="obligatorio">*</span></label>
      <Dropdown
        id="rol"
        v-model="form.rolId"
        :options="roles"
        optionLabel="label"
        optionValue="value"
        placeholder="Seleccionar rol"
        :disabled="deshabilitarRol"
      />

      <div v-if="errores.rolId" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.rolId }}
      </div>
    </div>

    <!-- Contraseña solo al crear -->
    <div class="campo" :class="{ error: errores.password }">
      <label for="password">
        Contraseña
        <span v-if="!form.id" class="obligatorio">*</span>
        <small
          v-if="form.id"
          style="font-weight: normal; font-size: 0.8rem; color: #666"
        >
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

    <div class="acciones-formulario">
      <div class="contenedor-boton">
        <Button
          class="btn-guardar"
          :label="form.id ? 'Actualizar' : 'Guardar'"
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
import Dropdown from "primevue/dropdown";
import Button from "primevue/button";
import authService from "../services/auth.service";

export default {
  name: "UsuarioForm",
  components: { InputText, Dropdown, Button },
  props: {
    usuario: Object,
    esPerfil: { type: Boolean, default: false },
  },
  data() {
    return {
      form: {
        id: null,
        nombre: "",
        email: "",
        telefono: "",
        rolId: null,
        password: "",
      },
      roles: [
        { label: "Administrador", value: 1 },
        { label: "Barbero", value: 2 },
      ],
      errores: {
        nombre: null,
        email: null,
        telefono: null,
        rolId: null,
        password: null,
      },
      usuarioLogueadoId: authService.getUser()?.id || null,
    };
  },
  mounted() {
    if (this.usuario) {
      this.form = {
        id: this.usuario.id,
        nombre: this.usuario.nombre || "",
        email: this.usuario.email || "",
        telefono: this.usuario.telefono || "",
        rolId: this.usuario.rolId || null,
        rolNombre: this.usuario.rolNombre || "",
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
    validarFormulario() {
      this.errores = {
        nombre: null,
        email: null,
        telefono: null,
        rolId: null,
        password: null,
      };
      let valido = true;

      if (!this.form.nombre.trim()) {
        this.errores.nombre = "El nombre es obligatorio.";
        valido = false;
      }
      if (!this.form.email.trim()) {
        this.errores.email = "El email es obligatorio.";
        valido = false;
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.form.email)) {
        this.errores.email = "El email no es válido.";
        valido = false;
      }
      if (this.form.telefono && /\D/.test(this.form.telefono)) {
        this.errores.telefono = "El teléfono debe contener solo números.";
        valido = false;
      }
      if (!this.esPerfil && !this.form.rolId) {
        this.errores.rolId = "El rol es obligatorio.";
        valido = false;
      }

      // Solo validar password si es nuevo usuario
      if (!this.form.id && !this.form.password) {
        this.errores.password = "La contraseña es obligatoria.";
        valido = false;
      }

      return valido;
    },
    onGuardar() {
      if (!this.validarFormulario()) return;
      console.warn("Formulario inválido", this.errores);

      const datos = { ...this.form };

      if (!datos.password) {
        delete datos.password; // No enviamos contraseña vacía para no modificarla
      }

      datos.accedeAlSistema = true; // siempre por defecto

      this.$emit("guardar", datos);
    },
  },
  computed: {
    deshabilitarRol() {
      return (
        this.usuarioLogueadoId !== null &&
        this.form.id === this.usuarioLogueadoId
      );
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

.p-button.p-button-danger {
  background-color: #e74c3c;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 0.5rem 1.2rem;
  transition: background-color 0.3s ease, box-shadow 0.3s ease;
}

.p-button.p-button-danger:hover {
  background-color: #c0392b;
  box-shadow: 0 0 8px #c0392b88;
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
</style>
