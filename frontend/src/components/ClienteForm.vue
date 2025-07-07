<template>
  <div class="formulario-cliente">
    <h3>{{ cliente?.id ? "Editar Cliente" : "Nuevo Cliente" }}</h3>

    <div class="campo" :class="{ error: errores.nombre }">
      <label for="nombre"> Nombre <span class="obligatorio">*</span> </label>
      <InputText id="nombre" v-model="form.nombre" />
      <div v-if="errores.nombre" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.nombre }}
      </div>
    </div>

    <div class="campo" :class="{ error: errores.email }">
      <label for="email">
        Email <span v-if="form.accedeAlSistema" class="obligatorio">*</span>
      </label>

      <InputText id="email" v-model="form.email" />
      <div v-if="errores.email" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.email }}
      </div>
    </div>

    <div class="campo" :class="{ error: errores.telefono }">
      <label for="telefono">
        Teléfono <span class="obligatorio">*</span>
      </label>
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

    <div class="acciones-formulario">
      <div class="contenedor-boton">
        <Button
          class="btn-guardar"
          :label="cliente?.id ? 'Actualizar' : 'Guardar'"
          icon="pi pi-check"
          @click="onGuardar"
        />
      </div>

      <button class="btn-cerrar" @click="$emit('cerrar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import InputText from "primevue/inputtext";
import Button from "primevue/button";

export default {
  name: "ClienteForm",
  components: {
    InputText,
    Button,
  },
  props: {
    cliente: {
      type: Object,
      default: null,
    },
  },
  emits: ["guardar", "cerrar"],
  data() {
    return {
      form: {
        nombre: "",
        email: "",
        telefono: "",
        rolId: 3,
        accedeAlSistema: false,
        password: null,
      },
      errores: {
        nombre: null,
        email: null,
        telefono: null,
      },
    };
  },
  watch: {
    cliente: {
      immediate: true,
      handler(newVal) {
        if (newVal) {
          Object.assign(this.form, {
            nombre: newVal.nombre || "",
            email: newVal.email || "",
            telefono: newVal.telefono || "",
            rolId: 3,
            accedeAlSistema: false,
            password: null,
          });
          this.limpiarErrores();
        } else {
          this.resetForm();
        }
      },
    },
  },
  methods: {
    soloNumeros(event) {
      const charCode = event.charCode ? event.charCode : event.keyCode;
      if (charCode < 48 || charCode > 57) {
        event.preventDefault();
      }
    },
    limpiarErrores() {
      this.errores = {
        nombre: null,
        email: null,
        telefono: null,
      };
    },
    validarEmail(email) {
      // Simple regex para validar email
      const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return re.test(email);
    },
    validarFormulario() {
      this.limpiarErrores();
      let valido = true;

      if (!this.form.nombre.trim()) {
        this.errores.nombre = "El nombre es obligatorio.";
        valido = false;
      }
      if (this.form.accedeAlSistema) {
        if (!this.form.email.trim()) {
          this.errores.email =
            "El email es obligatorio para usuarios con acceso.";
          valido = false;
        } else if (!this.validarEmail(this.form.email)) {
          this.errores.email = "El email no es válido.";
          valido = false;
        }
      } else if (
        this.form.email.trim() &&
        !this.validarEmail(this.form.email)
      ) {
        this.errores.email = "El email no es válido.";
        valido = false;
      }

      if (!this.form.telefono.trim()) {
        this.errores.telefono =
          "El teléfono es obligatorio y debe contener solo números.";
        valido = false;
      }

      return valido;
    },
    onGuardar() {
      if (!this.validarFormulario()) {
        return;
      }
      const payload = {
        id: this.cliente?.id ?? null,
        nombre: this.form.nombre,
        email: this.form.email,
        telefono: this.form.telefono,
        rolId: 3,
        accedeAlSistema: false,
      };
      this.$emit("guardar", payload);
    },
    resetForm() {
      this.form = {
        nombre: "",
        email: "",
        telefono: "",
        rolId: 3,
        accedeAlSistema: false,
        password: null,
      };
      this.limpiarErrores();
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
