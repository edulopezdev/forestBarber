<template>
  <div class="ventas-container">
    <Toast />
    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Ventas</h4>
          <div class="botones-acciones">
            <Button
              label="Filtros"
              icon="pi pi-filter"
              class="boton-filtros"
              @click="mostrarFiltros = !mostrarFiltros"
            />
            <Button
              label="Nueva Venta"
              icon="pi pi-plus"
              class="boton-nueva-venta"
              @click="crearVenta"
            />
          </div>
        </div>
      </template>
      <template #content>
        <DataTable
          v-model:filters="filters"
          :value="ventas"
          :filterDisplay="mostrarFiltros ? 'row' : 'none'"
          lazy
          paginator
          :rows="pageSize"
          :first="first"
          :totalRecords="totalVentas"
          tableStyle="min-width: 100%"
          :loading="loading"
          @page="onPageChange"
          @sort="onSort"
          @filter="onFilter"
        >
          <!-- Cliente -->
          <Column field="cliente" header="Cliente" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar cliente"
              />
            </template>
          </Column>

          <!-- Producto -->
          <Column field="productoNombres" header="Producto">
            <template #body="slotProps">
              {{ slotProps.data.producto }}
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar producto"
              />
            </template>
          </Column>

          <!-- Fecha -->
          <Column field="fecha" header="Fecha" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar fecha"
              />
            </template>
          </Column>

          <!-- Monto Pagado -->
          <Column field="montoPagado" header="Monto Total" sortable>
            <template #body="slotProps">
              {{
                new Intl.NumberFormat("es-AR", {
                  style: "currency",
                  currency: "ARS",
                }).format(slotProps.data.montoPagado || 0)
              }}
            </template>
          </Column>

          <!-- Estado -->
          <Column field="estado" header="Estado" sortable>
            <template #body="slotProps">
              <Tag
                :value="slotProps.data.estado ? 'Pagado' : 'Pendiente'"
                :severity="slotProps.data.estado ? 'success' : 'warning'"
                class="estado-etiqueta"
              />
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <Dropdown
                v-model="filterModel.value"
                @change="filterCallback()"
                :options="[
                  { label: 'Completada', value: true },
                  { label: 'Pendiente', value: false },
                ]"
                optionLabel="label"
                placeholder="Seleccionar estado"
                showClear
              />
            </template>
          </Column>

          <!-- Acciones -->
          <Column header="Acciones" style="min-width: 180px">
            <template #body="slotProps">
              <div class="acciones-botones">
                <Button
                  icon="pi pi-eye"
                  severity="info"
                  text
                  rounded
                  v-tooltip.bottom="'Ver detalles'"
                  @click="verDetalles(slotProps.data)"
                />
                <span
                  v-if="slotProps.data.estado"
                  v-tooltip.bottom="'Venta completada, no se puede editar'"
                >
                  <Button
                    icon="pi pi-pencil"
                    severity="warning"
                    text
                    rounded
                    disabled
                  />
                </span>
                <span v-else>
                  <Button
                    icon="pi pi-pencil"
                    severity="warning"
                    text
                    rounded
                    v-tooltip.bottom="'Editar venta'"
                    @click="editarVenta(slotProps.data)"
                  />
                </span>
                <!-- Botón pagar si está pendiente -->
                <Button
                  v-if="!slotProps.data.estado"
                  icon="pi pi-dollar"
                  severity="success"
                  text
                  rounded
                  v-tooltip.bottom="'Pagar venta'"
                  @click="pagarDialog(slotProps.data)"
                />
                <span
                  v-else
                  class="icono-check"
                  v-tooltip.bottom="'Venta pagada'"
                >
                  <i
                    class="pi pi-check"
                    style="color: #27ae60; font-size: 1.2rem"
                  ></i>
                </span>
              </div>
            </template>
          </Column>
        </DataTable>

        <!-- Mensaje total ventas -->
        <div class="total-ventas" v-if="totalVentas > 0">
          Total de ventas registradas: {{ totalVentas }}
        </div>
      </template>
    </Card>

    <!-- Modales -->
    <Dialog
      v-model:visible="mostrarModal"
      :header="ventaSeleccionada?.id ? 'Editar Venta' : 'Nueva Venta'"
      :modal="true"
      :closeOnEscape="false"
      :closable="false"
      style="width: 700px"
    >
      <VentaForm
        :venta="ventaSeleccionada"
        @guardar="guardarVenta"
        @cancelar="cerrarModal"
      />
    </Dialog>

    <Dialog
      v-model:visible="mostrarDetalleModal"
      header="Detalle de la Venta"
      :modal="true"
      :closable="false"
      style="width: 650px"
    >
      <div v-if="!ventaSeleccionada" class="mensaje-carga">
        <i class="pi pi-spin pi-spinner" style="font-size: 1.5rem"></i>
        <span>Cargando detalle...</span>
      </div>
      <VentaDetalle
        v-else
        :venta="ventaSeleccionada"
        @cerrar="mostrarDetalleModal = false"
      />
    </Dialog>

    <Dialog
      v-model:visible="mostrarPagarModal"
      header="Registrar Pago"
      :modal="true"
      :closable="false"
      style="width: 400px"
    >
      <div class="formulario-pago">
        <label for="metodo">Método de pago</label>
        <Dropdown
          id="metodo"
          v-model="pagoForm.metodo"
          :options="opcionesMetodoPago"
          optionLabel="nombre"
          optionValue="valor"
          placeholder="Seleccione método"
          class="mb-3 w-full"
        />
        <label
          >Monto a pagar (pendiente:
          {{ formatoMoneda(pagoForm.pendiente) }})</label
        >
        <InputText
          v-model="pagoForm.monto"
          type="text"
          placeholder="Ingrese monto"
          @input="validarMonto"
          class="w-full mb-2"
        />
        <small v-if="montoInvalido" class="p-error">{{ mensajeError }}</small>
        <div class="acciones-modal mt-4 flex justify-content-between">
          <Button
            label="Cancelar"
            icon="pi pi-times"
            severity="secondary"
            text
            class="p-button p-button-danger"
            @click="cerrarPagarModal"
          />
          <Button
            label="Registrar Pago"
            icon="pi pi-check"
            @click="registrarPago"
            :disabled="montoInvalido || !pagoForm.metodo"
          />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script>
import VentaService from "../services/VentaService";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Card from "primevue/card";
import Tag from "primevue/tag";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Dropdown from "primevue/dropdown";
import Dialog from "primevue/dialog";
import { FilterMatchMode } from "primevue/api";
import Swal from "sweetalert2";
import VentaForm from "../components/VentaForm.vue";
import VentaDetalle from "../components/VentaDetalle.vue";

export default {
  components: {
    DataTable,
    Column,
    Card,
    Tag,
    Button,
    InputText,
    Dropdown,
    Dialog,
    VentaForm,
    VentaDetalle,
  },
  data() {
    return {
      ventas: [],
      totalVentas: 0,
      currentPage: 1,
      pageSize: 10,
      first: 0,
      sortField: null,
      sortOrder: null,
      loading: false,
      mostrarFiltros: false,
      mostrarModal: false,
      mostrarDetalleModal: false,
      mostrarPagarModal: false,
      ventaSeleccionada: null,
      ventaSeleccionadaParaPago: null,
      pagoForm: {
        metodo: "",
        monto: "",
        pendiente: 0,
      },
      montoInvalido: false,
      mensajeError: "",
      opcionesMetodoPago: [
        { nombre: "Efectivo", valor: "Efectivo" },
        { nombre: "MercadoPago", valor: "MercadoPago" },
        { nombre: "NaranjaX", valor: "NaranjaX" },
      ],
      filters: {
        cliente: { value: null, matchMode: FilterMatchMode.CONTAINS },
        productoNombres: { value: null, matchMode: FilterMatchMode.CONTAINS },
        fecha: { value: null, matchMode: FilterMatchMode.CONTAINS },
        estado: { value: null, matchMode: FilterMatchMode.EQUALS },
      },
    };
  },
  mounted() {
    this.obtenerVentas();
  },
  methods: {
    async obtenerVentas(page = 1, pageSize = 10) {
      this.loading = true;
      try {
        const filtros = {
          clienteNombre: this.filters.cliente?.value || undefined,
          productoNombre: this.filters.productoNombres?.value || undefined,
          fecha: this.filters.fecha?.value || undefined,
          estadoPago:
            this.filters.estado?.value !== null
              ? this.filters.estado.value
                ? "pagado"
                : "pendiente"
              : null,
        };

        const ordenDescendente = this.sortOrder === -1;

        const res = await VentaService.getVentas(
          page,
          pageSize,
          filtros,
          this.sortField,
          ordenDescendente
        );

        this.ventas = res.data.ventas.map((venta) => {
          const pagos = venta.pagos || [];
          const montoPagado = pagos.reduce((acc, p) => acc + p.monto, 0);
          const estado = montoPagado >= venta.totalVenta;
          const nombresProductos = venta.detalles.map((d) => d.nombreProducto);
          return {
            cliente: venta.clienteNombre,
            producto: nombresProductos.join(", "),
            productoNombres: nombresProductos,
            fecha: new Date(venta.fechaAtencion).toLocaleDateString(),
            estado,
            id: venta.atencionId,
            totalVenta: venta.totalVenta,
            montoPagado,
          };
        });

        this.totalVentas = res.data.pagination?.total || this.ventas.length;
        this.currentPage = page;
        this.pageSize = pageSize;
        this.first = (page - 1) * pageSize;
      } catch (err) {
        console.error("Error al obtener ventas:", err);
        Swal.fire("Error", "No se pudieron cargar las ventas.", "error");
      } finally {
        this.loading = false;
      }
    },

    onPageChange(event) {
      this.obtenerVentas(event.page + 1, event.rows);
    },

    onSort(event) {
      this.sortField = event.sortField; // Guardamos el campo original (cliente, montoPagado, fecha)
      this.sortOrder = event.sortOrder;
      this.obtenerVentas(1, this.pageSize);
    },

    onFilter() {
      this.obtenerVentas(1, this.pageSize);
    },

    crearVenta() {
      this.ventaSeleccionada = this.nuevaVentaVacia();
      this.mostrarModal = true;
    },

    nuevaVentaVacia() {
      return {
        cliente: null,
        detalles: [],
      };
    },

    editarVenta(venta) {
      if (!venta.id) {
        Swal.fire(
          "Error",
          "No se encontró el ID de la venta para editar.",
          "error"
        );
        return;
      }

      this.loading = true;
      VentaService.getVentaById(venta.id)
        .then((res) => {
          const data = res.data.venta;
          if (!data.clienteId || data.clienteId <= 0) {
            Swal.fire("Error", "Cliente inválido o no encontrado.", "error");
            return;
          }
          if (!Array.isArray(data.detalles)) {
            Swal.fire("Error", "Detalles de venta no encontrados.", "error");
            return;
          }

          this.ventaSeleccionada = {
            id: data.atencionId,
            cliente: {
              id: data.clienteId,
              nombre: data.clienteNombre,
            },
            detalles: data.detalles.map((d) => ({
              productoServicioId: d.productoServicioId ?? 0,
              cantidad: d.cantidad ?? 1,
              precioUnitario: d.precioUnitario ?? 0.0,
              nombreProducto: d.nombreProducto ?? "Producto sin nombre",
            })),
            total: data.totalVenta ?? 0.0,
          };

          this.mostrarModal = true;
        })
        .catch((error) => {
          console.error("Error al cargar la venta:", error);
          Swal.fire(
            "Error",
            "No se pudo cargar la venta para editar.",
            "error"
          );
        })
        .finally(() => {
          this.loading = false;
        });
    },

    cerrarModal() {
      this.mostrarModal = false;
    },

    verDetalles(venta) {
      const atencionId = venta.id;
      if (!atencionId) {
        Swal.fire("Error", "ID de venta no encontrado.", "error");
        return;
      }

      VentaService.getVentaById(atencionId)
        .then((res) => {
          const data = res.data.venta;
          this.ventaSeleccionada = {
            ClienteNombre: data.clienteNombre,
            FechaAtencion: data.fechaAtencion,
            Detalles: data.detalles.map((d) => ({
              NombreProducto: d.nombreProducto,
              Cantidad: d.cantidad,
              PrecioUnitario: d.precioUnitario,
              Subtotal: d.subtotal,
              Observacion: d.observacion || "",
            })),
            TotalVenta: data.totalVenta,
            Pagos: data.pagos || [],
            AtencionId: atencionId,
          };
          this.mostrarDetalleModal = true;
        })
        .catch(() => {
          Swal.fire(
            "Error",
            "No se pudo cargar el detalle de la venta.",
            "error"
          );
        });
    },

    guardarVenta(data) {
      if (!data.cliente || !data.detalles || data.detalles.length === 0) {
        Swal.fire(
          "Error",
          "Debe seleccionar un cliente y al menos un producto.",
          "error"
        );
        return;
      }

      const payload = {
        clienteId: data.cliente.id,
        detalles: data.detalles.map((d) => ({
          productoServicioId: d.productoServicioId,
          cantidad: d.cantidad,
          precioUnitario: d.precioUnitario,
          observacion: d.observacion || null,
        })),
      };

      VentaService.crearVenta(payload)
        .then(() => {
          Swal.fire({
            icon: "success",
            title: "Venta registrada",
            background: "#18181b",
            color: "#fff",
            timer: 2000,
            showConfirmButton: false,
          });
          this.cerrarModal();
          this.obtenerVentas(this.currentPage, this.pageSize);
        })
        .catch((err) => {
          console.error("Error al guardar venta:", err);
          Swal.fire("Error", "No se pudo registrar la venta.", "error");
        });
    },

    pagarDialog(venta) {
      const pendiente = venta.totalVenta - (venta.montoPagado || 0);
      this.ventaSeleccionadaParaPago = venta;
      this.pagoForm = {
        metodo: "",
        monto: pendiente.toFixed(2),
        pendiente: pendiente,
      };
      this.montoInvalido = false;
      this.mensajeError = "";
      this.mostrarPagarModal = true;
    },

    validarMonto() {
      const monto = parseFloat(this.pagoForm.monto);
      if (isNaN(monto) || monto <= 0) {
        this.montoInvalido = true;
        this.mensajeError = "Ingrese un monto válido mayor a cero.";
        return;
      }
      if (monto > this.pagoForm.pendiente) {
        this.montoInvalido = true;
        this.mensajeError = `El monto no puede superar $${this.pagoForm.pendiente.toFixed(
          2
        )}.`;
        return;
      }
      this.montoInvalido = false;
      this.mensajeError = "";
    },

    async registrarPago() {
      this.validarMonto();
      if (this.montoInvalido || !this.pagoForm.metodo) return;

      const nuevoPago = {
        atencionId: this.ventaSeleccionadaParaPago.id,
        metodoPago: this.pagoForm.metodo,
        monto: parseFloat(this.pagoForm.monto),
        fecha: new Date().toISOString(),
      };

      try {
        await VentaService.RegistrarPago(nuevoPago);
        this.cerrarPagarModal();
        this.obtenerVentas(this.currentPage, this.pageSize);

        Swal.fire({
          icon: "success",
          title: "Pago registrado",
          text: `Método: ${
            nuevoPago.metodoPago
          }, Monto: $${nuevoPago.monto.toFixed(2)}`,
          background: "#18181b",
          color: "#fff",
          timer: 2000,
          showConfirmButton: false,
        });
      } catch (error) {
        console.error("Error al registrar pago:", error);
        Swal.fire("Error", "No se pudo registrar el pago.", "error");
      }
    },

    cerrarPagarModal() {
      this.mostrarPagarModal = false;
      this.ventaSeleccionadaParaPago = null;
      this.pagoForm = {
        metodo: "",
        monto: "",
        pendiente: 0,
      };
      this.montoInvalido = false;
      this.mensajeError = "";
    },

    formatoMoneda(valor) {
      return new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
      }).format(valor);
    },
  },
};
</script>

<style scoped>
/* ===========================
   CONTENEDOR GENERAL
=========================== */
.ventas-container {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  color: #e0e0e0;
}

/* ===========================
   ENCABEZADO DE TABLA Y BOTONES
=========================== */
.encabezado-acciones {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: -2.5rem;
  margin-top: -1rem;
}

.boton-filtros {
  background-color: transparent !important;
  color: white !important;
  border: none !important;
  box-shadow: none !important;
  padding: 0.3rem 0.6rem !important;
  margin-right: 1rem !important;
}
.boton-filtros:focus,
.boton-filtros:focus-visible,
.boton-filtros:active {
  outline: none !important;
  box-shadow: none !important;
  border-color: transparent !important;
}
:deep(.boton-filtros:focus-visible) {
  outline: none !important;
  box-shadow: 0 0 0 2px rgba(40, 167, 69, 0.4) !important;
  border-color: transparent !important;
}

.boton-nueva-venta {
  background-color: #28a745;
  color: white;
  font-weight: normal;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.7rem;
  width: auto !important;
  height: auto !important;
  min-width: 120px !important;
}
.boton-nueva-venta:hover {
  background-color: #218838;
}

/* ===========================
   DATATABLE ESTILOS GENERALES
=========================== */
:deep(.p-datatable) {
  background-color: #121212;
  color: #eee;
  border-radius: 10px;
  border: none;
  box-shadow: 0 3px 8px rgb(0 0 0 / 0.5);
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
  font-size: 0.95rem;
}

:deep(.p-datatable-tbody > tr > td),
:deep(.p-datatable-thead > tr > th) {
  padding: 0.4rem 0.75rem !important;
  line-height: 1.2rem;
  border-bottom: 1px solid #333 !important;
  vertical-align: middle;
  text-align: center !important;
}

:deep(.p-datatable-thead > tr > th) {
  background-color: #2a2a2a;
  color: #ffffff;
  font-weight: 600;
  text-align: center !important;
}

:deep(.p-datatable-tbody > tr:hover) {
  background-color: #1e3a1e;
}

:deep(.p-sortable-column .p-column-header-content) {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
}

:deep(.p-datatable-thead > tr > th:nth-child(1) .p-column-header-content) {
  gap: 0.1rem !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(5)) /* Estado */ {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(6)) /* Acciones */ {
  text-align: center !important;
  vertical-align: middle !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(6) .p-column-header-content) {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  gap: 0.4rem;
}

/* ===========================
   ESTILOS DE FILTROS EN COLUMNAS
=========================== */
:deep(.p-column-filter .p-inputtext) {
  height: 28px !important;
  padding: 0.25rem 0.5rem !important;
  font-size: 0.85rem !important;
  margin: 0 !important;
  border-radius: 4px !important;
}

:deep(.p-column-filter .p-dropdown) {
  height: 28px !important;
  font-size: 0.85rem !important;
}

:deep(.p-column-filter .p-dropdown .p-dropdown-label) {
  line-height: 28px !important;
  padding: 0 0.5rem !important;
}

:deep(.p-column-filter) {
  padding: 0.15rem 0.3rem !important;
  margin: 0 !important;
  min-height: 40px;
}

:deep(.p-column-filter-menu-button) {
  background: transparent !important;
  border: none !important;
  padding: 0 !important;
  width: 14px;
  height: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

:deep(.p-column-filter-clear-button) {
  background: transparent !important;
  border: none !important;
  padding: 0 !important;
  width: auto !important;
  height: auto !important;
  min-width: 0 !important;
  min-height: 0 !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  box-shadow: none !important;
  outline: none !important;
}

:deep(.p-column-filter-clear-button > svg) {
  width: 16px !important;
  height: 16px !important;
  pointer-events: auto;
  fill: currentColor;
}

:deep(.p-column-filter-menu-button:focus),
:deep(.p-column-filter-menu-button:active) {
  outline: none !important;
  box-shadow: none !important;
}

/* ===========================
   PAGINADOR
=========================== */
:deep(.p-datatable .p-paginator) {
  background-color: transparent;
  border-top: 1px solid #444;
  padding-top: 0.4rem;
  font-size: 0.85rem;
  color: #ccc;
}

:deep(.p-datatable .p-paginator .p-highlight) {
  background-color: #28a745 !important;
  color: white !important;
  border-radius: 4px;
  font-weight: 600;
}

:deep(.p-paginator .p-paginator-page:focus),
:deep(.p-paginator .p-paginator-page:focus-visible),
:deep(.p-paginator .p-paginator-page.p-highlight) {
  outline: none !important;
  box-shadow: none !important;
  border-color: transparent !important;
}

/* ===========================
   BOTONES DE ACCIÓN EN FILA
=========================== */
.acciones-botones {
  display: flex;
  gap: 0.25rem;
  justify-content: center;
  align-items: center;
}

:deep(.p-button) {
  margin: 0 !important;
  padding: 0.25rem 0.35rem !important;
  font-size: 0.85rem !important;
  width: 30px;
  height: 30px;
  min-width: 30px !important;
  border-radius: 6px !important;
  line-height: 1;
}

:deep(.p-button:last-child) {
  margin-right: 0;
}

/* ===========================
   TAGS DE ESTADO (ACTIVO / INACTIVO)
=========================== */
:deep(.p-tag) {
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
  font-weight: 600;
}

:deep(.p-tag-success) {
  background-color: #27ae60 !important;
  color: #e0f2f1 !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}
:deep(.p-tag-warning) {
  background-color: #f0ad4e !important;
  color: #000000 !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}

:deep(.p-tag-danger) {
  background-color: #c0392b !important;
  color: #fdecea !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}

/* ===========================
   TÍTULOS Y FILTROS AVANZADOS
=========================== */
.titulo-columna {
  font-weight: 600;
  color: white;
}

.columna-nombre-header {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 0.3rem;
}

.icono-filtro {
  position: absolute;
  left: -5rem;
  top: 50%;
  transform: translateY(-50%);
  color: white;
  font-size: 1rem;
  cursor: pointer;
  transition: transform 0.2s ease, color 0.2s ease;
  z-index: 10;
}
.icono-filtro:hover {
  transform: scale(1.2);
  color: #28a745;
}

.total-ventas {
  margin-top: 1.9rem;
  font-size: 1rem;
  font-weight: 500;
  text-align: left;
  color: #aeaeae;
}
.icono-check {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 30px;
}

/* Estilos para formulario de pago */
.formulario-pago {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.mb-2 {
  margin-bottom: 0.5rem;
}
.mb-3 {
  margin-bottom: 1rem;
}
.w-full {
  width: 100%;
}
.acciones-modal {
  margin-top: 1rem;
}
.estado-etiqueta {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 80px; /* Mismo tamaño para ambos */
  padding: 0.25rem 0.5rem !important; /* Padding uniforme */
  font-size: 0.85rem !important; /* Tamaño de texto igual */
  border-radius: 12px !important; /* Bordes consistentes */
  text-align: center;
}
</style>
