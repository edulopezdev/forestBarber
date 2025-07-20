<template>
  <div class="detalle-venta">
    <h3 class="titulo"><i class="pi pi-shopping-cart"></i> Detalle de Venta</h3>

    <!-- Información básica -->
    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-id-card"></i> Cliente</label>
        <p>{{ venta?.ClienteNombre }}</p>
      </div>
      <div class="campo">
        <label><i class="pi pi-calendar"></i> Fecha de Atención</label>
        <p>{{ formatearFecha(venta?.FechaAtencion) }}</p>
      </div>
    </div>

    <div class="campo">
      <label><i class="pi pi-credit-card"></i> Pagos Realizados</label>
      <div v-if="venta?.Pagos?.length">
        <Tag v-for="pago in venta.Pagos" :key="pago.pagoId" severity="success">
          {{ pago.metodoPago }} -
          {{
            new Intl.NumberFormat("es-MX", {
              style: "currency",
              currency: "MXN",
            }).format(pago.monto)
          }}
        </Tag>
      </div>
      <Tag v-else severity="warning">Sin pagos realizados</Tag>
    </div>

    <!-- Detalles de productos/servicios -->
    <div class="seccion">
      <h4><i class="pi pi-list"></i> Productos y Servicios</h4>
      <table class="tabla-detalles">
        <thead>
          <tr>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Precio Unitario</th>
            <th>Subtotal</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(detalle, index) in venta?.Detalles" :key="index">
            <td>{{ detalle.NombreProducto }}</td>
            <td>{{ detalle.Cantidad }}</td>
            <td>$ {{ detalle.PrecioUnitario.toFixed(2) }}</td>
            <td>$ {{ detalle.Subtotal.toFixed(2) }}</td>
            <td>
              <button
                class="btn-icono"
                @click="abrirObservacion(detalle.Observacion)"
                aria-label="Ver observación"
              >
                <i
                  class="pi pi-book"
                  :style="{ opacity: detalle.observacion?.trim() ? 1 : 0.4 }"
                ></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Totales -->
    <div class="seccion">
      <div class="campo">
        <label><i class="pi pi-dollar"></i> Total de Venta</label>
        <p>
          <strong>$ {{ venta?.TotalVenta.toFixed(2) }}</strong>
        </p>
      </div>

      <div class="campo">
        <label><i class="pi pi-credit-card"></i> Estado del Pago</label>
        <Tag
          v-if="venta?.Pagos?.length && cajaCerrada"
          severity="info"
        >
          Cerrado
        </Tag>
        <Tag
          v-else-if="venta?.Pagos?.length"
          :severity="totalPagado >= venta.TotalVenta ? 'success' : 'info'"
        >
          {{ totalPagado >= venta.TotalVenta ? "Completada" : "Parcial" }}
        </Tag>
        <Tag v-else severity="warning">Pendiente</Tag>
      </div>
    </div>

    <!-- Acciones finales -->
    <div class="acciones-formulario">
      <button class="btn-cerrar" @click="$emit('cerrar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>

    <!-- Modal de Observación individual -->
    <Dialog
      v-model:visible="mostrarModalObservacion"
      :modal="true"
      :closable="false"
      :dismissableMask="true"
      :style="{ width: '400px' }"
    >
      <template #header>
        <div
          style="
            display: flex;
            justify-content: space-between;
            align-items: center;
          "
        >
          <span>Observación</span>
          <button
            class="btn-cerrar"
            @click="mostrarModalObservacion = false"
            aria-label="Cerrar"
          >
            <i class="pi pi-times"></i>
          </button>
        </div>
      </template>

      <p>{{ observacionSeleccionada }}</p>
    </Dialog>
  </div>
</template>

<script>
import Tag from "primevue/tag";
import Dialog from "primevue/dialog";

export default {
  name: "VentaDetalle",
  components: { Tag, Dialog },
  props: {
    venta: {
      type: Object,
      default: null,
    },
    cajaCerrada: {
      type: Boolean,
      default: false
    }
  },
  emits: ["cerrar"],
  data() {
    return {
      mostrarModalObservacion: false,
      observacionSeleccionada: "",
    };
  },
  mounted() {
    console.log("Venta completa:", this.venta);
  },
  methods: {
    formatearFecha(fecha) {
      if (!fecha) return "No disponible";
      const d = new Date(fecha);
      return d.toLocaleDateString("es-AR", {
        day: "2-digit",
        month: "long",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      });
    },
    abrirObservacion(observacion) {
      if (!observacion || !observacion.trim()) {
        this.observacionSeleccionada = "No posee observaciones.";
      } else {
        this.observacionSeleccionada = observacion;
      }
      this.mostrarModalObservacion = true;
    },
  },
  computed: {
    totalPagado() {
      return this.venta?.Pagos?.reduce((acc, pago) => acc + pago.monto, 0) || 0;
    },
  },
};
</script>

<style scoped>
.detalle-venta {
  max-width: 700px;
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

.tabla-detalles {
  width: 100%;
  border-collapse: collapse;
  margin-top: 0.5rem;
}

.tabla-detalles th,
.tabla-detalles td {
  text-align: left;
  padding: 0.5rem;
  border-bottom: 1px solid #333;
}

.tabla-detalles th {
  background-color: #2c2c2c;
  color: #f0f0f0;
}

.acciones-formulario {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 2rem;
}

.btn-cerrar {
  background-color: transparent;
  border: none;
  color: #ccc;
  font-size: 1.2rem;
  cursor: pointer;
}

.btn-icono {
  background-color: transparent;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
  color: #ccc;
}

.btn-icono:hover {
  color: #fff;
}
</style>
