<template>
  <div class="detalle-producto">
    <h3 class="titulo">
      <i :class="esServicio ? 'pi pi-cog' : 'pi pi-box'"></i>
      Detalle del {{ esServicio ? "Servicio" : "Producto" }}
    </h3>

    <!-- Sección: Nombre y Descripción -->
    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-tag"></i> Nombre</label>
        <p>{{ producto?.nombre }}</p>
      </div>

      <div class="campo">
        <label><i class="pi pi-info-circle"></i> Descripción</label>
        <p>{{ producto?.descripcion || "Sin descripción" }}</p>
      </div>
    </div>

    <!-- Sección: Precio y Cantidad -->
    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-dollar"></i> Precio</label>
        <p>{{ formatoPrecio(producto?.precio) }}</p>
      </div>

      <div class="campo" v-if="!esServicio">
        <label><i class="pi pi-inbox"></i> Cantidad en stock</label>
        <p>{{ producto?.cantidad }}</p>
      </div>
    </div>

    <!-- Sección: Imagen -->
    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-image"></i> Imagen</label>
        <div class="imagen-contenedor">
          <img
            v-if="producto?.rutaImagen"
            :src="getRutaImagen(producto.rutaImagen)"
            alt="Imagen del producto"
            class="imagen-detalle"
          />
          <img
            v-else
            src="/img/no-image.jpg"
            alt="Sin imagen"
            class="imagen-detalle"
          />
        </div>
      </div>
    </div>

    <!-- Botón de cierre -->
    <div class="acciones-formulario">
      <button class="btn-cerrar" @click="$emit('cerrar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
export default {
  name: "ProductoServicioDetalle",
  props: {
    producto: {
      type: Object,
      required: true,
    },
    esServicio: {
      type: Boolean,
      default: false,
    },
  },
  emits: ["cerrar"],
  methods: {
    // Formatea la fecha a un formato legible
    formatearFecha(fecha) {
      if (!fecha) return "No se modificó nunca";
      const d = new Date(fecha);
      return d.toLocaleDateString("es-AR", {
        day: "2-digit",
        month: "long",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
    },

    // Formato de moneda USD
    formatoPrecio(precio) {
      return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      }).format(precio);
    },

    // Obtiene la URL completa de la imagen
    getRutaImagen(ruta) {
      if (!ruta) return "/img/no-image.jpg";
      const baseUrl =
        import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";
      return ruta.startsWith("http") ? ruta : `${baseUrl}${ruta}`;
    },
  },
};
</script>

<style scoped>
.detalle-producto {
  max-width: 500px;
  margin: 0 auto;
  padding: 1.5rem;
  background-color: #1e1e1e;
  border-radius: 12px;
  color: #f0f0f0;
  font-family: "Segoe UI", sans-serif;
}

.titulo {
  font-size: 1.5rem;
  margin-bottom: 1.2rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #ffffff;
  border-bottom: 1px solid #333;
  padding-bottom: 0.5rem;
}

.seccion {
  margin-bottom: 1.5rem;
}

.campo {
  margin-bottom: 0.8rem;
}

label {
  font-weight: 600;
  font-size: 0.95rem;
  color: #bbbbbb;
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.3rem;
}

p {
  margin: 0;
  font-size: 0.95rem;
  color: #eeeeee;
}

.imagen-contenedor {
  display: flex;
  justify-content: center;
  margin-top: 0.5rem;
}

.imagen-detalle {
  width: 100%;
  height: 150px;
  object-fit: cover;
  border-radius: 8px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.5);
}

.acciones-formulario {
  display: flex;
  justify-content: flex-end;
  margin-top: 2rem;
}

.btn-cerrar {
  background-color: transparent;
  border: none;
  color: #ff4d4d;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.3s ease;
}

.btn-cerrar:hover {
  color: #ff1a1a;
}
</style>
