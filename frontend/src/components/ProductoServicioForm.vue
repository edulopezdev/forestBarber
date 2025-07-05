<template>
  <div class="formulario-producto">
    <h3>
      {{
        form.id
          ? esServicio
            ? "Editar Servicio"
            : "Editar Producto"
          : esServicio
          ? "Nuevo Servicio"
          : "Nuevo Producto"
      }}
    </h3>

    <!-- Nombre -->
    <div class="campo" :class="{ error: errores.nombre }">
      <label for="nombre">Nombre <span class="obligatorio">*</span></label>
      <InputText id="nombre" v-model="form.nombre" autofocus />
      <div v-if="errores.nombre" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.nombre }}
      </div>
    </div>

    <!-- Descripción -->
    <div class="campo" :class="{ error: errores.descripcion }">
      <label for="descripcion">Descripción</label>
      <Textarea id="descripcion" v-model="form.descripcion" rows="3" />
      <div v-if="errores.descripcion" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.descripcion }}
      </div>
    </div>

    <!-- Precio -->
    <div class="campo" :class="{ error: errores.precio }">
      <label for="precio">Precio <span class="obligatorio">*</span></label>
      <InputNumber
        id="precio"
        v-model="form.precio"
        mode="currency"
        currency="USD"
        locale="en-US"
      />
      <div v-if="errores.precio" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.precio }}
      </div>
    </div>

    <!-- Es Almacenable (solo visible si se llama desde ambas vistas) -->
    <div class="campo" v-if="almacenablePorDefecto === null">
      <label for="almacenable">Es almacenable</label>
      <Checkbox id="almacenable" v-model="form.esAlmacenable" :binary="true" />
    </div>

    <!-- Cantidad (solo visible si es almacenable) -->
    <div
      class="campo"
      v-if="form.esAlmacenable"
      :class="{ error: errores.cantidad }"
    >
      <label for="cantidad">Cantidad</label>
      <InputNumber id="cantidad" v-model="form.cantidad" />
      <div v-if="errores.cantidad" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.cantidad }}
      </div>
    </div>

    <!-- Imagen -->
    <div class="campo">
      <label>Imagen</label>
      <div class="avatar-preview">
        <img :src="imagenUrl" alt="Imagen del producto" />
      </div>
      <div class="avatar-actions">
        <FileUpload
          mode="basic"
          name="imagen"
          accept="image/*"
          chooseLabel=""
          chooseIcon="pi pi-pencil"
          @select="onImagenSeleccionada"
          :auto="true"
          customUpload
          class="boton-avatar fileupload-icon"
        />
        <Button
          icon="pi pi-trash"
          class="boton-avatar boton-avatar-eliminar"
          @click="eliminarImagen"
          v-if="form.imagen || previewImagen"
        />
      </div>
    </div>

    <!-- Botones -->
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
import Textarea from "primevue/textarea";
import InputNumber from "primevue/inputnumber";
import Checkbox from "primevue/checkbox";
import Button from "primevue/button";
import FileUpload from "primevue/fileupload";

export default {
  name: "ProductoServicioForm",
  components: {
    InputText,
    Textarea,
    InputNumber,
    Checkbox,
    Button,
    FileUpload,
  },
  props: {
    productoServicio: Object,
    almacenablePorDefecto: {
      type: Boolean,
      default: null,
    },
    esServicio: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      form: {
        id: null,
        nombre: "",
        descripcion: "",
        precio: 0.0,
        esAlmacenable: this.almacenablePorDefecto ?? false,
        cantidad: 0,
        imagen: "",
        nuevoArchivo: null,
        eliminarImagenFlag: false,
      },
      previewImagen: null,
      errores: {
        nombre: null,
        descripcion: null,
        precio: null,
        cantidad: null,
      },
    };
  },
  mounted() {
    if (this.productoServicio) {
      this.form = {
        id: this.productoServicio.id,
        nombre: this.productoServicio.nombre || "",
        descripcion: this.productoServicio.descripcion || "",
        precio: this.productoServicio.precio || 0.0,
        esAlmacenable: this.productoServicio.esAlmacenable ?? false,
        cantidad: this.productoServicio.cantidad ?? 0,
        imagen: this.productoServicio.rutaImagen || "",
        nuevoArchivo: null,
        eliminarImagenFlag: false,
      };
    } else {
      // Si no hay datos, usamos el valor por defecto
      this.form.esAlmacenable = this.almacenablePorDefecto ?? false;
    }
  },
  methods: {
    onImagenSeleccionada(event) {
      const file = event.files[0];
      if (file) {
        this.form.nuevoArchivo = file;
        this.previewImagen = URL.createObjectURL(file);
      }
    },
    eliminarImagen() {
      this.form.imagen = null;
      this.form.nuevoArchivo = null;
      this.previewImagen = null;
      this.form.eliminarImagenFlag = true;
    },
    validarFormulario() {
      this.errores = {
        nombre: null,
        descripcion: null,
        precio: null,
        cantidad: null,
      };

      let valido = true;

      if (!this.form.nombre.trim()) {
        this.errores.nombre = "El nombre es obligatorio.";
        valido = false;
      }

      if (this.form.precio <= 0) {
        this.errores.precio = "El precio debe ser mayor a cero.";
        valido = false;
      }

      if (this.form.esAlmacenable && this.form.cantidad < 0) {
        this.errores.cantidad = "La cantidad no puede ser negativa.";
        valido = false;
      }

      if (!this.form.esAlmacenable && this.form.cantidad > 0) {
        this.errores.cantidad = "Un servicio no puede tener cantidad.";
        valido = false;
      }

      return valido;
    },
    onGuardar() {
      if (!this.validarFormulario()) return;

      const formData = new FormData();

      if (this.form.id !== null) {
        formData.append("Id", this.form.id);
      }

      formData.append("Nombre", this.form.nombre);
      formData.append("Descripcion", this.form.descripcion || "");
      formData.append("Precio", this.form.precio);
      formData.append("EsAlmacenable", this.form.esAlmacenable);

      if (this.form.esAlmacenable) {
        formData.append("Cantidad", this.form.cantidad);
      }

      if (this.form.nuevoArchivo) {
        formData.append("Imagen", this.form.nuevoArchivo);
      }

      if (this.form.eliminarImagenFlag) {
        formData.append("EliminarImagen", "true");
      }

      this.$emit("guardar", formData);
    },
  },
  computed: {
    imagenUrl() {
      if (this.previewImagen) return this.previewImagen;
      if (this.form.imagen) {
        const baseUrl =
          import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";
        return this.form.imagen.startsWith("http")
          ? this.form.imagen
          : `${baseUrl}${this.form.imagen}`;
      }
      return "/img/no-image.png"; // placeholder por defecto
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
