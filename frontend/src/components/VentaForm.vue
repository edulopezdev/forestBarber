<template>
  <div class="formulario-venta" v-if="formularioVisible">
    <!-- Título -->
    <h3>{{ ventaId ? "Editar Atención" : "Nueva Atención" }}</h3>

    <!-- Selección de Cliente -->
    <div class="campo" :class="{ error: errores.cliente }">
      <label for="cliente">
        Seleccionar Cliente <span class="obligatorio">*</span>
      </label>
      <AutoComplete
        v-model="formulario.cliente"
        :suggestions="clientesFiltrados"
        @complete="buscarClientes"
        placeholder="Selecciona un cliente"
        :force-selection="true"
        class="auto-complete-fullwidth"
      >
        <template #option="slotProps">
          <span v-html="resaltarCoincidenciaDinamica(slotProps.option.nombre)"></span>
        </template>
      </AutoComplete>
      <div v-if="errores.cliente" class="error-msg">
        <i class="pi pi-exclamation-triangle"></i> {{ errores.cliente }}
      </div>
    </div>

    <!-- Búsqueda de producto -->
    <div class="campo">
      <label for="busqueda">Buscar Producto o Servicio</label>
      <InputText
        v-model="busquedaProducto"
        @input="filtrarProductoServicios({ query: busquedaProducto })"
        placeholder="Escribe para buscar..."
      />
    </div>

    <!-- Lista de resultados -->
    <div class="lista-productos" v-if="productos.length > 0">
      <div
        v-for="producto in productos"
        :key="producto.id"
        class="producto-item"
        @click="agregarAlCarrito(producto)"
      >
        {{ producto.nombre }} - ${{ producto.precio }}
        <small>({{ producto.esAlmacenable ? "Producto" : "Servicio" }})</small>
      </div>
    </div>

    <!-- Carrito (tabla siempre visible) -->
    <div class="carrito">
      <h4>Productos Seleccionados</h4>
      <table class="tabla-carrito">
        <thead>
          <tr>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Precio Unitario</th>
            <th>Total</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="carrito.length === 0">
            <td
              colspan="5"
              style="text-align: center; font-style: italic; color: #999"
            >
              No hay productos agregados
            </td>
          </tr>
          <tr v-for="(item, index) in carrito" :key="index">
            <td>{{ item.nombreProducto }}</td>
            <td class="cantidad-control">
              <div v-if="item.esAlmacenable" class="boton-cantidad">
                <Button
                  icon="pi pi-minus"
                  text
                  @click="disminuirCantidad(item)"
                />
                <span class="valor-cantidad">{{ item.cantidad }}</span>
                <Button
                  icon="pi pi-plus"
                  text
                  @click="aumentarCantidad(item)"
                />
              </div>
              <div v-else class="valor-cantidad">1</div>
            </td>

            <td>$ {{ item.precioUnitario.toFixed(2) }}</td>
            <td>$ {{ (item.cantidad * item.precioUnitario).toFixed(2) }}</td>
            <td>
              <button
                @click="abrirNotaModal(index)"
                class="btn-editar-nota"
                aria-label="Editar nota"
              >
                <i class="pi pi-pencil"></i>
              </button>
              <button
                @click="eliminarDelCarrito(index)"
                class="btn-eliminar-item"
                aria-label="Eliminar"
              >
                <i class="pi pi-trash"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <div class="total" v-if="carrito.length > 0">
        <strong>Total: ${{ totalCarrito.toFixed(2) }}</strong>
      </div>
    </div>

    <Dialog
      v-model:visible="notaModal.visible"
      header="Observación"
      :modal="true"
      :closable="true"
      :style="{ width: '400px' }"
    >
      <div class="campo">
        <label for="nota">Nota para el producto</label>
        <Textarea v-model="notaModal.texto" rows="4" autoResize />
      </div>
      <template #footer>
        <Button
          label="Cancelar"
          icon="pi pi-times"
          @click="cerrarNotaModal"
          class="p-button-text"
        />
        <Button
          label="Guardar"
          icon="pi pi-check"
          @click="guardarNota"
          autofocus
        />
      </template>
    </Dialog>

    <!-- Botones Guardar y Cerrar -->
    <div class="acciones-formulario">
      <Button
        class="btn-guardar"
        :label="
          guardando
            ? ventaId
              ? 'Actualizando...'
              : 'Guardando...'
            : ventaId
            ? 'Actualizar'
            : 'Guardar'
        "
        icon="pi pi-check"
        @click="onGuardar"
        :disabled="guardando"
      >
        <template #icon>
          <span v-if="guardando" class="spinner-guardar">
            <i class="pi pi-spin pi-spinner"></i>
          </span>
          <i v-else class="pi pi-check"></i>
        </template>
      </Button>
      <button class="btn-cerrar" @click="$emit('cancelar')" aria-label="Cerrar">
        <i class="pi pi-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import InputText from "primevue/inputtext";
import InputNumber from "primevue/inputnumber";
import AutoComplete from "primevue/autocomplete";
import Button from "primevue/button";
import UsuarioService from "../services/UsuarioService";
import VentaService from "../services/VentaService";
import Dialog from "primevue/dialog";
import Textarea from "primevue/textarea";

export default {
  name: "VentaForm",
  components: {
    InputText,
    InputNumber,
    AutoComplete,
    Button,
    Dialog,
    Textarea,
  },
  props: {
    venta: {
      type: [String, Number, Object, null],
      default: null,
    },
  },
  data() {
    return {
      guardando: false,
      formularioVisible: true,
      ventaId: this.venta?.id ? parseInt(this.venta.id) : null,
      formulario: {
        cliente: null,
      },
      clientesFiltrados: [],
      productos: [], // Productos o servicios obtenidos desde backend
      carrito: [],
      busquedaProducto: "",
      errores: {
        cliente: null,
      },
      notaModal: {
        visible: false,
        index: null,
        texto: "",
      },
      busquedaCliente: "", // Nueva propiedad para almacenar la búsqueda del cliente
    };
  },
  computed: {
    totalCarrito() {
      return this.carrito.reduce(
        (acc, item) => acc + item.cantidad * item.precioUnitario,
        0
      );
    },
  },
  watch: {
    venta(nuevoValor) {
      if (nuevoValor && nuevoValor !== null) {
        console.log("Cliente recibido:", nuevoValor.cliente);

        this.formulario.cliente = nuevoValor.cliente
          ? JSON.parse(JSON.stringify(nuevoValor.cliente))
          : null;

        this.carrito = nuevoValor.detalles
          ? nuevoValor.detalles.map((detalle) => ({
              productoServicioId: detalle.productoServicioId,
              cantidad: detalle.cantidad,
              precioUnitario: detalle.precioUnitario,
              nombreProducto:
                detalle.nombreProducto ||
                detalle.nombre ||
                "Producto sin nombre",
              esAlmacenable: detalle.esAlmacenable ?? false,
            }))
          : [];
      } else {
        this.limpiarFormulario();
      }
    },
  },
  created() {
    if (this.venta && this.venta.detalles) {
      this.formulario.cliente = this.venta.cliente
        ? JSON.parse(JSON.stringify(this.venta.cliente))
        : null;

      this.carrito = this.venta.detalles.map((detalle) => ({
        productoServicioId: detalle.productoServicioId,
        cantidad: detalle.cantidad,
        precioUnitario: detalle.precioUnitario,
        nombreProducto:
          detalle.nombreProducto || detalle.nombre || "Producto sin nombre",
        esAlmacenable: detalle.esAlmacenable ?? false,
      }));
    }
  },

  methods: {
    async buscarClientes(event) {
      this.busquedaCliente = event.query || ""; // Guarda el texto buscado
      const query = (event.query || "").trim().toLowerCase();
      if (!query || query.length < 1) {
        this.clientesFiltrados = [];
        return;
      }
      try {
        const response = await UsuarioService.getClientes(1, 20, {
          nombre: query,
          activo: true,
        });
        // Filtrar en frontend: nombre completo (nombre + apellido) que empiece alguna palabra con el query
        this.clientesFiltrados =
          (response.data.clientes || []).filter((c) => {
            const nombreCompleto = ((c.nombre || "") + " " + (c.apellido || "")).trim().toLowerCase();
            // Coincidencia al inicio de cualquier palabra del nombre completo
            return nombreCompleto.split(" ").some((w) => w.startsWith(query));
          }).map((c) => ({
            id: c.id,
            nombre: ((c.nombre || "") + " " + (c.apellido || "")).trim(),
          }));
      } catch (error) {
        console.error("Error al buscar clientes:", error);
        this.clientesFiltrados = [];
      }
    },
    aumentarCantidad(item) {
      item.cantidad += 1;
    },
    disminuirCantidad(item) {
      if (item.cantidad > 1) {
        item.cantidad -= 1;
      }
    },
    async filtrarProductoServicios(event) {
      const query = event.query || "";
      if (!query || query.length < 1) {
        this.productos = [];
        return;
      }

      try {
        const res = await VentaService.getProductosServiciosVenta(1, 10, query);

        this.productos = res.data.productos.map((p) => ({
          id: p.id,
          nombre: p.nombre,
          precio: p.precio,
          esAlmacenable: p.esAlmacenable,
        }));
      } catch (error) {
        console.error("Error al buscar productos/servicios:", error);
        this.productos = [];
      }
    },

    agregarAlCarrito(producto) {
      if (!producto.id || producto.id === 0) {
        alert("Error: El producto seleccionado no tiene un ID válido.");
        return;
      }

      const existe = this.carrito.find((p) => p.id === producto.id);
      if (existe) {
        existe.cantidad += 1;
      } else {
        this.carrito.push({
          productoServicioId: producto.id,
          nombreProducto: producto.nombre,
          nombre: producto.nombre,
          cantidad: 1,
          precioUnitario: producto.precio,
          esAlmacenable: producto.esAlmacenable,
        });
      }
      this.busquedaProducto = "";
      this.productos = [];
    },
    eliminarDelCarrito(index) {
      this.carrito.splice(index, 1);
    },

    editarNota(index) {
      alert(`Editar nota del producto índice ${index}`);
      // Aquí puedes implementar la lógica para editar la nota
    },

    validarFormulario() {
      this.errores = { cliente: null };

      if (this.carrito.length === 0) {
        alert("Debe agregar al menos un producto o servicio.");
        return false;
      }

      if (!this.formulario.cliente) {
        this.errores.cliente = "Debe seleccionar un cliente.";
        return false;
      }

      return true;
    },

    onGuardar() {
      if (!this.validarFormulario()) return;

      this.guardando = true; // Deshabilitar botón y mostrar feedback

      const datos = {
        cliente: this.formulario.cliente,
        detalles: this.carrito,
        total: this.totalCarrito,
        id: this.ventaId,
      };
      console.log("Datos a enviar al backend:", JSON.stringify(datos, null, 2));
      // Emitir evento y esperar respuesta del padre para re-habilitar el botón
      this.$emit("guardar", datos);

      // El padre debe emitir un evento para re-habilitar el botón (ver VentasView.vue)
      // Como fallback, re-habilitar después de 10s si no hay respuesta
      setTimeout(() => {
        this.guardando = false;
      }, 10000);
    },

    limpiarFormulario() {
      this.formulario.cliente = null;
      this.carrito = [];
      this.busquedaProducto = "";
      this.productos = [];
      this.errores = { cliente: null };
    },
    abrirNotaModal(index) {
      const item = this.carrito[index];
      this.notaModal = {
        visible: true,
        index,
        texto: item.observacion || "",
      };
    },
    cerrarNotaModal() {
      this.notaModal.visible = false;
      this.notaModal.index = null;
      this.notaModal.texto = "";
    },
    guardarNota() {
      if (this.notaModal.index !== null) {
        this.carrito[this.notaModal.index].observacion = this.notaModal.texto;
      }
      this.cerrarNotaModal();
    },
    resaltarCoincidenciaDinamica(nombreCompleto) {
  const query = (this.busquedaCliente || "").trim();
  if (!query) return nombreCompleto;
  // Resalta todas las coincidencias, insensible a mayúsculas/minúsculas
  const regex = new RegExp(`(${query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  return nombreCompleto.replace(regex, '<span class="highlight">$1</span>');
    },
  },
};
</script>

<style scoped>
.formulario-venta {
  max-width: 700px;
  margin: 0 auto;
  padding: 1rem;
  color: #f0f0f0;
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
}

.lista-productos {
  max-height: 200px;
  overflow-y: auto;
  margin-bottom: 1rem;
  border: 1px solid #444;
  border-radius: 6px;
  background-color: #1e1e1e;
  padding: 0.5rem;
}

.producto-item {
  padding: 0.5rem;
  cursor: pointer;
  border-bottom: 1px solid #333;
  transition: background-color 0.2s ease;
}

.producto-item:hover {
  background-color: #2a2a2a;
}

.tabla-carrito {
  width: 100%;
  border-collapse: collapse;
  margin-top: 0.5rem;
  background-color: #1f1f1f;
  border-radius: 6px;
  overflow: hidden;
}

.tabla-carrito th,
.tabla-carrito td {
  padding: 0.75rem;
  text-align: left;
  border-bottom: 1px solid #333;
}

.tabla-carrito th {
  background-color: #292929;
  color: #ccc;
  font-weight: 600;
}

.total {
  margin-top: 1rem;
  font-size: 1.1rem;
  text-align: right;
}

.acciones-formulario {
  display: flex;
  justify-content: space-between;
  margin-top: 1.5rem;
}

.btn-cerrar {
  background-color: transparent;
  border: none;
  color: #f0f0f0;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 0.4rem;
  border-radius: 4px;
  transition: background-color 0.2s ease;
}

.btn-cerrar:hover {
  background-color: #2a2a2a;
  color: #fff;
}

.btn-eliminar-item {
  background: transparent;
  border: none;
  cursor: pointer;
  color: #dc3545; /* rojo para eliminar */
  font-size: 1.2rem;
  padding: 0.2rem 0.6rem;
  margin-left: 0.5rem;
}

.btn-eliminar-item:hover {
  color: #ff6b6b;
}
.auto-complete-fullwidth {
  width: 100% !important;
}

.auto-complete-fullwidth {
  width: 100% !important;
}

.auto-complete-fullwidth :deep(.p-autocomplete-input) {
  width: 100% !important;
  box-sizing: border-box;
}

.boton-cantidad {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.boton-cantidad .p-button {
  color: #28a745 !important;
  padding: 0 6px !important;
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.boton-cantidad .p-button:hover {
  background-color: rgba(40, 167, 69, 0.1);
}

.valor-cantidad {
  font-weight: bold;
  min-width: 24px;
  text-align: center;
  user-select: none;
}
.btn-editar-nota {
  background-color: transparent;
  border: none;
  color: #007bff;
  font-size: 1.2rem;
  padding: 0.2rem 0.6rem;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.btn-editar-nota:hover {
  background-color: rgba(0, 123, 255, 0.1);
}

.btn-editar-nota:focus,
.btn-editar-nota:active {
  outline: none;
  box-shadow: none;
  background-color: transparent !important;
}

.spinner-guardar {
  display: inline-flex;
  align-items: center;
  margin-right: 0.3rem;
  font-size: 1.1em;
}

.highlight {
  background: #ffe066;
  color: #222;
  font-weight: bold;
  border-radius: 3px;
  padding: 0 2px;
}
</style>