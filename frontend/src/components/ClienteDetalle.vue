<template>
  <div class="detalle-cliente">
    <h3 class="titulo"><i class="pi pi-user"></i> Detalle del Cliente</h3>

    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-id-card"></i> Nombre</label>
        <p>{{ cliente?.nombre }}</p>
      </div>

      <div class="campo">
        <label><i class="pi pi-envelope"></i> Email</label>
        <p>{{ cliente?.email }}</p>
      </div>

      <div class="campo">
        <label><i class="pi pi-phone"></i> Teléfono</label>
        <p>{{ cliente?.telefono || "No disponible" }}</p>
      </div>
    </div>

    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-shield"></i> Estado</label>
        <Tag
          :value="cliente?.activo ? 'Activo' : 'Inactivo'"
          :severity="cliente?.activo ? 'success' : 'danger'"
        />
      </div>

      <div class="campo">
        <label><i class="pi pi-key"></i> Accede al sistema</label>
        <p>{{ cliente?.accedeAlSistema ? "Sí" : "No" }}</p>
      </div>
    </div>

    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-calendar-plus"></i> Fecha de registro</label>
        <p>{{ formatearFecha(cliente?.fechaRegistro) }}</p>
      </div>

      <div class="campo">
        <label><i class="pi pi-refresh"></i> Última modificación</label>
        <p>{{ formatearFecha(cliente?.fechaModificacion) }}</p>
      </div>
    </div>

    <div class="acciones-formulario">
      <button class="btn-cerrar" @click="$emit('cerrar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import Tag from "primevue/tag";
import Button from "primevue/button";

export default {
  name: "ClienteDetalle",
  components: { Tag, Button },
  props: {
    cliente: Object,
  },
  emits: ["cerrar"],
  methods: {
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
  },
};
</script>

<style scoped>
.detalle-cliente {
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

.acciones-formulario {
  display: flex;
  justify-content: flex-end;
  margin-top: 2rem;
}
</style>
