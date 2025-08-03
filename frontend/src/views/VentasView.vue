<template>
  <div class="clientes-container">
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
        <TablaGlobal
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
          :sortField="sortField"
          :sortOrder="sortOrder"
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
          <Column
            field="productoNombres"
            header="Producto"
            :headerStyle="{ textAlign: 'center' }"
          >
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
              <Calendar
                v-model="filters.fecha.value"
                @update:modelValue="onFechaChange"
                dateFormat="dd/mm/yy"
                placeholder="Seleccionar fecha"
                showIcon
                style="width: 140px; min-width: 120px"
                class="w-full filtro-fecha"
                inputClass="w-full"
              />
            </template>
          </Column>

          <!-- Monto Total -->
          <Column field="totalVenta" header="Monto Total" sortable>
            <template #body="slotProps">
              {{
                new Intl.NumberFormat("es-AR", {
                  style: "currency",
                  currency: "ARS",
                }).format(slotProps.data.totalVenta || 0)
              }}
            </template>
          </Column>

          <!-- Estado -->
          <Column
            field="estado"
            header="Estado"
            :sortable="false"
            :headerStyle="{ textAlign: 'center' }"
          >
            <template #body="slotProps">
              <Tag
                :value="getEstadoVenta(slotProps.data)"
                :severity="getEstadoSeverity(slotProps.data)"
                class="estado-etiqueta"
              />
            </template>
<template #filter="{ filterModel, filterCallback }">
              <div style="display: flex; justify-content: center; width: 100%;">
                <Dropdown
                  v-model="filters.estado.value"
                  @change="onEstadoChange"
                  :options="[
                    { label: 'Pagado', value: true },
                    { label: 'Pendiente', value: false },
                    { label: 'Cerrado', value: 'cerrado' },
                  ]"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Seleccionar estado"
                  showClear
                  style="width: 150px; min-width: 150px;"
                />
              </div>
            </template>
          </Column>

          <!-- Acciones -->
          <Column header="Acciones" style="min-width: 180px; text-align: center">
            <template #body="slotProps">
              <div class="acciones-botones">
                <!-- Espacio adicional a la izquierda -->
                <div class="espacio-izquierda"></div>
                <Button
                  icon="pi pi-eye"
                  severity="info"
                  text
                  rounded
                  v-tooltip.bottom="'Ver detalles'"
                  @click="verDetalles(slotProps.data)"
                />
                <!-- Botón de editar: habilitado si la caja no está cerrada, sin importar el estado de pago -->
                <span v-if="estaCajaCerrada(slotProps.data)">
                  <Button
                    icon="pi pi-pencil"
                    severity="secondary"
                    text
                    rounded
                    disabled
                    class="button-disabled"
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
                <!-- Botón pagar: habilitado si la caja no está cerrada, incluso si ya está pagada -->
                <span v-if="estaCajaCerrada(slotProps.data)">
                  <span
                    class="icono-check"
                    v-tooltip.bottom="{
                      value: slotProps.data.estado
                        ? 'Venta cerrada'
                        : 'No se puede registrar pagos: la venta está cerrada',
                      class: 'tooltip-error',
                    }"
                  >
                    <i
                      :class="
                        slotProps.data.estado ? 'pi pi-lock' : 'pi pi-dollar'
                      "
                      :style="{
                        color: slotProps.data.estado ? '#3498db' : '#999',
                        fontSize: '1.2rem',
                      }"
                    ></i>
                  </span>
                </span>
                <span v-else>
                  <Button
                    icon="pi pi-dollar"
                    :severity="slotProps.data.estado ? 'info' : 'success'"
                    text
                    rounded
                    v-tooltip.bottom="
                      slotProps.data.estado ? 'Modificar pagos' : 'Pagar venta'
                    "
                    @click="pagarDialog(slotProps.data)"
                  />
                </span>
                <!-- Botón eliminar: solo para ventas con estado "Pendiente" -->
                <span
                  v-if="
                    !slotProps.data.estado && !estaCajaCerrada(slotProps.data)
                  "
                >
                  <Button
                    icon="pi pi-trash"
                    severity="danger"
                    text
                    rounded
                    v-tooltip.bottom="'Eliminar venta'"
                    @click="confirmarEliminarVenta(slotProps.data)"
                  />
                </span>
              </div>
            </template>
          </Column>
        </TablaGlobal>

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
        :cajaCerrada="
          ventaSeleccionada && ventaSeleccionada.CierreDiarioId != null
        "
        @cerrar="mostrarDetalleModal = false"
      />
    </Dialog>

    <Dialog
      v-model:visible="mostrarPagarModal"
      header="Registrar Pago"
      :modal="true"
      :closable="false"
      style="width: 750px"
      class="modal-pago"
    >
      <template #icons>
        <Button
          icon="pi pi-times"
          class="p-dialog-header-icon p-dialog-header-close p-link"
          @click="verificarCierreSinGuardar"
        />
      </template>
      <div class="formulario-pago compact-modal">
        <!-- Formulario de nuevo pago -->
        <h4 class="compact-title">Nuevo Pago</h4>
        <div class="form-row">
          <div class="form-group">
            <div class="label-container">
              <label for="metodo"
                >Método de pago <span class="required-indicator">*</span></label
              >
              <small
                v-if="!pagoForm.metodo && pagosPendientes.length === 0"
                class="metodo-hint"
                >Seleccione para habilitar el botón</small
              >
            </div>
            <Dropdown
              id="metodo"
              v-model="pagoForm.metodo"
              :options="opcionesMetodoPago"
              optionLabel="nombre"
              optionValue="valor"
              placeholder="Seleccione método"
              class="dropdown-metodo-pago"
              :class="{
                'p-dropdown-highlight':
                  !pagoForm.metodo && pagosPendientes.length === 0,
              }"
              appendTo="self"
            />
          </div>
          <div class="form-group">
            <label>Monto a pagar</label>
            <InputText
              v-model="pagoForm.monto"
              type="text"
              placeholder="Ingrese monto"
              @input="validarMonto"
            />
          </div>
          <div class="form-group btn-container">
            <Button
              label="Agregar Pago"
              icon="pi pi-plus"
              class="btn-agregar-pago"
              @click="agregarPagoTemporal"
              :disabled="
                montoInvalido ||
                !pagoForm.metodo ||
                totalPagado >= ventaSeleccionadaParaPago?.totalVenta ||
                (ventaSeleccionadaParaPago &&
                  estaCajaCerrada(ventaSeleccionadaParaPago))
              "
              v-tooltip.bottom="getAgregarPagoTooltip()"
            />
          </div>
        </div>
        <small v-if="montoInvalido" class="p-error">{{ mensajeError }}</small>

        <!-- Separador -->
        <div class="separador compact-separator"></div>

        <!-- Lista de pagos registrados -->
        <h4 class="compact-title">Pagos Registrados</h4>
        <div v-if="pagosCargando" class="pagos-cargando">
          <i class="pi pi-spin pi-spinner"></i> Cargando pagos...
        </div>
        <div
          v-else-if="
            pagosRegistrados.length === 0 && pagosPendientes.length === 0
          "
          class="sin-pagos"
        >
          No hay pagos registrados
        </div>
        <div v-else class="lista-pagos">
          <!-- Pagos ya registrados en la BD (no se pueden modificar) -->
          <div
            v-for="pago in pagosRegistrados"
            :key="'reg-' + pago.pagoId"
            class="pago-item pago-registrado"
          >
            <div class="pago-info">
              <div class="pago-metodo">{{ pago.metodoPago }}</div>
              <div class="pago-monto">{{ formatoMoneda(pago.monto) }}</div>
              <div class="pago-fecha">{{ formatearFecha(pago.fecha) }}</div>
            </div>
            <!-- Botón de eliminar pago: deshabilitado si la venta está cerrada -->
            <span
              v-if="
                ventaSeleccionadaParaPago &&
                estaCajaCerrada(ventaSeleccionadaParaPago)
              "
              v-tooltip.bottom="{
                value: 'No se puede eliminar: la venta está cerrada',
                class: 'tooltip-error',
              }"
            >
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                disabled
                class="button-disabled"
                style="width: 2.5rem; height: 2.5rem"
              />
            </span>
            <span v-else>
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                style="width: 2.5rem; height: 2.5rem"
                @click="confirmarEliminarPago(pago)"
              />
            </span>
          </div>

          <!-- Pagos pendientes de confirmar (temporales) -->
          <div
            v-for="(pago, index) in pagosPendientes"
            :key="'temp-' + index"
            class="pago-item pago-pendiente"
          >
            <div class="pago-info">
              <div class="pago-metodo">{{ pago.metodoPago }}</div>
              <div class="pago-monto">{{ formatoMoneda(pago.monto) }}</div>
              <div class="pago-fecha pago-pendiente-badge">
                Pendiente de confirmar
              </div>
            </div>
            <!-- Botón de eliminar pago temporal: deshabilitado si la venta está cerrada -->
            <span
              v-if="
                ventaSeleccionadaParaPago &&
                estaCajaCerrada(ventaSeleccionadaParaPago)
              "
              v-tooltip.bottom="{
                value: 'No se puede eliminar: la venta está cerrada',
                class: 'tooltip-error',
              }"
            >
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                disabled
                class="button-disabled"
                style="width: 2.5rem; height: 2.5rem"
              />
            </span>
            <span v-else>
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                style="width: 2.5rem; height: 2.5rem"
                @click="eliminarPagoTemporal(index)"
              />
            </span>
          </div>
        </div>

        <!-- Información de total pagado y monto restante en una sola línea -->
        <div class="info-total-pagado" v-if="ventaSeleccionadaParaPago">
          <div class="info-row">
            <div class="info-item">
              <div class="info-label">Total pagado:</div>
              <div
                class="info-value"
                :class="{
                  'total-completo':
                    totalPagado >= ventaSeleccionadaParaPago?.totalVenta,
                }"
              >
                {{ formatoMoneda(totalPagado) }}
                <span
                  v-if="totalPagado >= ventaSeleccionadaParaPago?.totalVenta"
                  class="badge-completo"
                  >COMPLETO</span
                >
              </div>
            </div>
            <div
              class="info-item"
              v-if="totalPagado < ventaSeleccionadaParaPago?.totalVenta"
            >
              <div class="info-label">Monto restante:</div>
              <div class="info-value pendiente">
                {{
                  formatoMoneda(
                    ventaSeleccionadaParaPago.totalVenta - totalPagado
                  )
                }}
              </div>
            </div>
          </div>
        </div>

        <!-- Botones de acción -->
        <div class="acciones-modal mt-4">
          <Button
            label="Cancelar"
            icon="pi pi-times"
            severity="secondary"
            text
            class="p-button p-button-danger"
            @click="cancelarPagos"
          />
          <Button
            label="Confirmar y Guardar"
            icon="pi pi-check"
            class="p-button p-button-success btn-confirmar"
            @click="mostrarConfirmacionPagos"
            :disabled="
              pagosPendientes.length === 0 ||
              (ventaSeleccionadaParaPago &&
                estaCajaCerrada(ventaSeleccionadaParaPago))
            "
            v-tooltip.bottom="
              ventaSeleccionadaParaPago &&
              estaCajaCerrada(ventaSeleccionadaParaPago)
                ? {
                    value:
                      'No se pueden confirmar pagos: la venta está cerrada',
                    class: 'tooltip-error',
                  }
                : null
            "
          />
        </div>
      </div>
    </Dialog>

    <!-- No se necesita modal de confirmación, se usa SweetAlert2 -->
  </div>
</template>

<script>
import VentaService from "../services/VentaService";
import CajaService from "../services/CajaService";
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
import Calendar from "primevue/calendar";

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
    Calendar,
  },
  data() {
    return {
      ventas: [],
      totalVentas: 0,
      currentPage: 1,
      pageSize: 6,
      first: 0,
      sortField: "fecha",
      sortOrder: -1, // -1 para descendente, 1 para ascendente
      loading: false,
      mostrarFiltros: false,
      mostrarModal: false,
      mostrarDetalleModal: false,
      mostrarPagarModal: false,
      ventaSeleccionada: null,
      ventaOriginal: null, // Para guardar todos los datos originales de la venta
      ventaSeleccionadaParaPago: null,
      pagoForm: {
        metodo: "",
        monto: "",
        pendiente: 0,
      },
      montoInvalido: false,
      mensajeError: "",
      pagosRegistrados: [], // Pagos ya registrados en la BD
      pagosPendientes: [], // Pagos temporales pendientes de confirmar
      pagosCargando: false,
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
      totalPagado: 0, // Total pagado incluyendo pagos pendientes
      fechaActual: new Date().toISOString().split("T")[0], // Fecha actual en formato YYYY-MM-DD
    };
  },
  mounted() {
    this.obtenerVentas();
  },
  watch: {
    // Watcher para mostrarModal
    mostrarModal(nuevoValor) {
      if (nuevoValor && this.ventaSeleccionada) {
        // Si se intenta abrir el modal y la venta tiene fecha
        if (
          this.ventaSeleccionada.id &&
          this.estaCajaCerrada(this.ventaSeleccionada)
        ) {
          // Cerrar el modal inmediatamente
          this.$nextTick(() => {
            this.mostrarModal = false;
          });

          Swal.fire({
            icon: "error",
            title: "Operación no permitida",
            text: "No se puede editar esta venta porque ya está cerrada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
          });
        }
      }
    },
    // Watcher para mostrarPagarModal
    mostrarPagarModal(nuevoValor) {
      if (nuevoValor && this.ventaSeleccionadaParaPago) {
        // Si se intenta abrir el modal y la venta tiene fecha
        if (this.estaCajaCerrada(this.ventaSeleccionadaParaPago)) {
          // Cerrar el modal inmediatamente
          this.$nextTick(() => {
            this.mostrarPagarModal = false;
          });

          Swal.fire({
            icon: "error",
            title: "Operación no permitida",
            text: "No se pueden registrar pagos para esta venta porque ya está cerrada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
          });
        }
      }
    },
  },
  methods: {
    onFechaChange() {
      // Al seleccionar una fecha, dispara el filtro y GET
      this.obtenerVentas(1, this.pageSize);
    },
    onEstadoChange() {
      // Al cambiar el estado, dispara el filtro y GET
      this.obtenerVentas(1, this.pageSize);
    },
    async obtenerVentas(page = 1, pageSize = this.pageSize) {
      this.loading = true;

      try {
        // Formatear fecha a yyyy-MM-dd si es Date
        let fechaFiltro = undefined;
        if (this.filters.fecha?.value) {
          if (this.filters.fecha.value instanceof Date) {
            const d = this.filters.fecha.value;
            fechaFiltro = `${d.getFullYear()}-${String(
              d.getMonth() + 1
            ).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
          } else {
            fechaFiltro = this.filters.fecha.value;
          }
        }
        // Estado: null si no hay filtro, "completo", "pendiente" o "cerrado" si hay filtro
        let estadoFiltro = null;
        if (this.filters.estado?.value === true) estadoFiltro = "completo";
        else if (this.filters.estado?.value === false)
          estadoFiltro = "pendiente";
        else if (this.filters.estado?.value === "cerrado")
          estadoFiltro = "cerrado";

        const filtros = {
          clienteNombre: this.filters.cliente?.value || undefined,
          productoNombre: this.filters.productoNombres?.value || undefined,
          fecha: fechaFiltro,
          estadoPago: estadoFiltro,
        };
        const ordenDescendente = this.sortOrder === -1;
        const ordenCampo = this.getBackendSortField(this.sortField);
        const res = await VentaService.getVentas(
          page,
          pageSize,
          filtros,
          ordenCampo,
          ordenDescendente
        );

        this.ventas = res.data.ventas.map((venta) => {
          const pagos = venta.pagos || [];
          const montoPagado = pagos.reduce((acc, p) => acc + p.monto, 0);
          const estado = montoPagado >= venta.totalVenta;
          const nombresProductos = venta.detalles.map((d) => d.nombreProducto);

          // Ya no necesitamos verificar el estado de la caja por fecha
          // El estado cerrado viene directamente en el campo cierreDiarioId

          return {
            cliente: venta.clienteNombre,
            producto: nombresProductos.join(", "),
            productoNombres: nombresProductos,
            fecha: new Date(venta.fechaAtencion).toLocaleDateString(),
            fechaOriginal: venta.fechaAtencion, // Guardar la fecha original
            estado,
            id: venta.atencionId,
            totalVenta: venta.totalVenta,
            montoPagado,
            pagos: venta.pagos, // Guardar los pagos para referencia
            cierreDiarioId: venta.cierreDiarioId, // Guardar el ID del cierre diario
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

    // Traduce el campo de la tabla al campo esperado por el backend
    getBackendSortField(field) {
      switch (field) {
        case "cliente":
          return "cliente";
        case "fecha":
          return "fecha";
        case "totalVenta":
          return "monto";
        default:
          return "fecha";
      }
    },

    onPageChange(event) {
      this.obtenerVentas(event.page + 1, event.rows);
    },

    onSort(event) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder;
      this.obtenerVentas(1, this.pageSize);
    },

    onFilter() {
      this.obtenerVentas(1, this.pageSize);
    },

    crearVenta() {
      // Crear un objeto venta temporal con la fecha actual para verificar
      const ventaTemporal = {
        fechaOriginal: this.fechaActual,
      };

      // Verificar si la caja del día actual está cerrada
      if (!this.verificarModalPermitido(ventaTemporal)) {
        return;
      }

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
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "No se encontró el ID de la venta para editar.",
          background: "#18181b",
          color: "#fff",
        });
        return;
      }

      // Verificar si la caja está cerrada para esta venta
      if (!this.verificarModalPermitido(venta)) {
        return;
      }

      this.loading = true;
      VentaService.getVentaById(venta.id)
        .then((res) => {
          const data = res.data.venta;
          if (!data.clienteId || data.clienteId <= 0) {
            Swal.fire({
              icon: "error",
              title: "Error",
              text: "Cliente inválido o no encontrado.",
              background: "#18181b",
              color: "#fff",
            });
            return;
          }
          if (!Array.isArray(data.detalles)) {
            Swal.fire({
              icon: "error",
              title: "Error",
              text: "Detalles de venta no encontrados.",
              background: "#18181b",
              color: "#fff",
            });
            return;
          }

          // Guardar la venta original completa para mantener todos los datos
          this.ventaOriginal = data;

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
          Swal.fire({
            icon: "error",
            title: "Error",
            text: "No se pudo cargar la venta para editar.",
            background: "#18181b",
            color: "#fff",
          });
        })
        .finally(() => {
          this.loading = false;
        });
    },

    cerrarModal() {
      this.mostrarModal = false;
      this.ventaOriginal = null;
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
            CierreDiarioId: data.cierreDiarioId,
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
      // Verificar si la venta está pagada y se intenta eliminar un producto
      if (
        data.id &&
        this.ventaOriginal &&
        this.ventaOriginal.pagos.length > 0
      ) {
        const productosOriginales = this.ventaOriginal.detalles.map(
          (d) => d.productoServicioId
        );
        const productosActuales = data.detalles.map(
          (d) => d.productoServicioId
        );

        const productosEliminados = productosOriginales.filter(
          (id) => !productosActuales.includes(id)
        );

        if (productosEliminados.length > 0) {
          Swal.fire({
            icon: "warning",
            title: "Acción no permitida",
            text: "Debe eliminar los pagos registrados antes de eliminar productos de una venta pagada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
          });
          return;
        }
      }

      // Verificar si es una edición o una nueva venta
      if (data.id) {
        // Para ediciones, verificar si la caja está cerrada
        // Buscar la venta en la lista para verificar si la caja está cerrada
        const ventaExistente = this.ventas.find((v) => v.id === data.id);
        if (ventaExistente && !this.verificarModalPermitido(ventaExistente)) {
          Swal.fire({
            icon: "error",
            title: "Operación no permitida",
            text: "No se puede editar esta venta porque ya está cerrada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
            customClass: {
              container: "swal-container-class",
            },
          });
          return;
        }

        // Es una edición, mostrar confirmación primero
        Swal.fire({
          title: "¿Modificar esta venta?",
          text: "Se actualizarán los datos de la venta seleccionada",
          icon: "question",
          showCancelButton: true,
          confirmButtonColor: "#28a745",
          cancelButtonColor: "#6c757d",
          confirmButtonText: "Sí, modificar",
          cancelButtonText: "Cancelar",
          background: "#18181b",
          color: "#fff",
          customClass: {
            container: "swal-container-class",
          },
        }).then((result) => {
          if (result.isConfirmed) {
            // Preparar solo los detalles para actualizar
            const detalles = data.detalles.map((d) => ({
              productoServicioId: d.productoServicioId,
              cantidad: d.cantidad,
              precioUnitario: d.precioUnitario,
              observacion: d.observacion || null,
            }));

            // Usar el nuevo método que solo actualiza los detalles
            VentaService.actualizarDetallesVenta(data.id, detalles)
              .then(() => {
                Swal.fire({
                  icon: "success",
                  title: "Venta actualizada",
                  background: "#18181b",
                  color: "#fff",
                  timer: 2000,
                  showConfirmButton: false,
                  customClass: {
                    container: "swal-container-class",
                  },
                });
                this.cerrarModal();
                this.obtenerVentas(this.currentPage, this.pageSize);
              })
              .catch((err) => {
                console.error("Error al actualizar venta:", err);
                Swal.fire({
                  icon: "error",
                  title: "Error",
                  text: "No se pudo actualizar la venta.",
                  background: "#18181b",
                  color: "#fff",
                  customClass: {
                    container: "swal-container-class",
                  },
                });
              });
          }
        });
      } else {
        // Para ventas nuevas, verificar si la caja del día actual está cerrada
        const ventaTemporal = { fechaOriginal: this.fechaActual };
        if (!this.verificarModalPermitido(ventaTemporal)) {
          Swal.fire({
            icon: "error",
            title: "Operación no permitida",
            text: "No se puede crear una nueva venta porque la caja del día actual está cerrada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
            customClass: {
              container: "swal-container-class",
            },
          });
          return;
        }

        // Es una nueva venta, usar el método de creación
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
              customClass: {
                container: "swal-container-class",
              },
            });
            this.cerrarModal();
            this.obtenerVentas(this.currentPage, this.pageSize);
          })
          .catch((err) => {
            console.error("Error al guardar venta:", err);
            const errorMessage = err.response?.data?.message || "No se pudo registrar la venta.";
            Swal.fire({
              icon: "warning",
              title: "Atención",
              text: errorMessage,
              background: "#18181b",
              color: "#fff",
              customClass: {
                container: "swal-container-class",
              },
            });
          });
      }
    },

    pagarDialog(venta) {
      // Verificar si la caja está cerrada para esta venta
      if (!this.verificarModalPermitido(venta)) {
        return;
      }

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
      this.pagosPendientes = []; // Limpiar pagos pendientes
      this.totalPagado = venta.montoPagado || 0; // Inicializar con lo ya pagado

      // Cargar los pagos existentes
      this.cargarPagosExistentes(venta.id);
    },

    async cargarPagosExistentes(atencionId) {
      this.pagosCargando = true;
      try {
        // En lugar de usar el endpoint específico de pagos, usamos el mismo endpoint
        // que se usa en verDetalles, que ya sabemos que funciona
        const response = await VentaService.getVentaById(atencionId);
        console.log("Respuesta de venta con pagos:", response.data);

        // Extraer los pagos de la venta
        if (response.data && response.data.venta && response.data.venta.pagos) {
          this.pagosRegistrados = response.data.venta.pagos;
          // Actualizar el total pagado con los pagos registrados
          const montoPagadoRegistrado = this.pagosRegistrados.reduce(
            (acc, p) => acc + p.monto,
            0
          );
          this.totalPagado = montoPagadoRegistrado;

          // Actualizar el pendiente en el formulario
          const pendiente =
            this.ventaSeleccionadaParaPago.totalVenta - this.totalPagado;
          this.pagoForm.pendiente = pendiente > 0 ? pendiente : 0;
          this.pagoForm.monto = this.pagoForm.pendiente.toFixed(2);
        } else {
          console.error("No se encontraron pagos en la respuesta");
          this.pagosRegistrados = [];
        }

        console.log("Pagos registrados:", this.pagosRegistrados);
      } catch (error) {
        console.error("Error al cargar pagos:", error);
        this.pagosRegistrados = [];
      } finally {
        this.pagosCargando = false;
      }
    },

    validarMonto() {
      const monto = parseFloat(this.pagoForm.monto);
      if (isNaN(monto) || monto <= 0) {
        this.montoInvalido = true;
        this.mensajeError = "Ingrese un monto válido mayor a cero.";
        return;
      }

      // Calcular el total que se pagaría con este nuevo pago
      const totalConNuevoPago = this.totalPagado + monto;

      // Verificar que no exceda el total de la venta
      if (totalConNuevoPago > this.ventaSeleccionadaParaPago.totalVenta) {
        this.montoInvalido = true;
        this.mensajeError = `El monto excede el total pendiente. Máximo: $${(
          this.ventaSeleccionadaParaPago.totalVenta - this.totalPagado
        ).toFixed(2)}.`;
        return;
      }

      this.montoInvalido = false;
      this.mensajeError = "";
    },

    // Método para agregar un pago temporal (no se guarda en BD todavía)
    agregarPagoTemporal() {
      // Verificar si la caja está cerrada para la venta seleccionada
      if (
        this.ventaSeleccionadaParaPago &&
        this.estaCajaCerrada(this.ventaSeleccionadaParaPago)
      ) {
        Swal.fire({
          icon: "error",
          title: "Operación no permitida",
          text: "No se pueden agregar pagos porque la venta está cerrada.",
          background: "#18181b",
          color: "#fff",
          confirmButtonColor: "#dc3545",
        });
        return;
      }

      this.validarMonto();
      if (this.montoInvalido || !this.pagoForm.metodo) return;

      // Verificar que no se exceda el total
      const monto = parseFloat(this.pagoForm.monto);
      const nuevoTotal = this.totalPagado + monto;

      if (nuevoTotal > this.ventaSeleccionadaParaPago.totalVenta) {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "El monto total de pagos excedería el valor de la venta.",
          background: "#18181b",
          color: "#fff",
        });
        return;
      }

      // Crear pago temporal
      const nuevoPago = {
        atencionId: this.ventaSeleccionadaParaPago.id,
        metodoPago: this.pagoForm.metodo,
        monto: monto,
        fecha: new Date().toISOString(),
      };

      // Agregar a la lista de pagos pendientes
      this.pagosPendientes.push(nuevoPago);

      // Actualizar el total pagado
      this.totalPagado += monto;

      // Actualizar el pendiente
      const pendiente =
        this.ventaSeleccionadaParaPago.totalVenta - this.totalPagado;
      this.pagoForm.pendiente = pendiente > 0 ? pendiente : 0;

      // Limpiar el formulario para un nuevo pago
      this.pagoForm.metodo = "";
      this.pagoForm.monto =
        this.pagoForm.pendiente > 0
          ? this.pagoForm.pendiente.toFixed(2)
          : "0.00";

      // Ya no mostramos mensaje de éxito al agregar un pago
    },

    // Método para eliminar un pago temporal
    eliminarPagoTemporal(index) {
      // Verificar si la venta está cerrada
      if (
        this.ventaSeleccionadaParaPago &&
        this.estaCajaCerrada(this.ventaSeleccionadaParaPago)
      ) {
        Swal.fire({
          icon: "error",
          title: "Operación no permitida",
          text: "No se pueden eliminar pagos porque la venta está cerrada.",
          background: "#18181b",
          color: "#fff",
          confirmButtonColor: "#dc3545",
        });
        return;
      }

      if (index < 0 || index >= this.pagosPendientes.length) return;

      const pago = this.pagosPendientes[index];

      // Actualizar el total pagado
      this.totalPagado -= pago.monto;

      // Eliminar el pago de la lista
      this.pagosPendientes.splice(index, 1);

      // Actualizar el pendiente
      const pendiente =
        this.ventaSeleccionadaParaPago.totalVenta - this.totalPagado;
      this.pagoForm.pendiente = pendiente;
      this.pagoForm.monto = pendiente.toFixed(2);

      // Mostrar mensaje
      Swal.fire({
        icon: "info",
        title: "Pago eliminado",
        background: "#18181b",
        color: "#fff",
        timer: 1500,
        showConfirmButton: false,
      });
    },

    // Método para registrar un pago in
    async registrarPago(pago) {
      try {
        await VentaService.RegistrarPago(pago);
        return true;
      } catch (error) {
        console.error("Error al registrar pago:", error);
        throw error;
      }
    },

    async actualizarVentaSeleccionada(actualizarTabla = true) {
      try {
        // Obtener la venta actualizada
        const response = await VentaService.getVentaById(
          this.ventaSeleccionadaParaPago.id
        );
        const venta = response.data.venta;

        // Calcular el monto pagado y pendiente
        const pagos = venta.pagos || [];
        const montoPagado = pagos.reduce((acc, p) => acc + p.monto, 0);
        const pendiente = venta.totalVenta - montoPagado;

        // Actualizar la venta seleccionada
        this.ventaSeleccionadaParaPago = {
          ...this.ventaSeleccionadaParaPago,
          montoPagado,
          estado: montoPagado >= venta.totalVenta,
        };

        // Actualizar el monto pendiente en el formulario
        this.pagoForm.pendiente = pendiente;
        this.pagoForm.monto = pendiente > 0 ? pendiente.toFixed(2) : "0.00";

        // Actualizar el estado en la tabla de ventas inmediatamente
        const ventaIndex = this.ventas.findIndex(
          (v) => v.id === this.ventaSeleccionadaParaPago.id
        );
        if (ventaIndex !== -1) {
          this.ventas[ventaIndex].estado = montoPagado >= venta.totalVenta;
          this.ventas[ventaIndex].montoPagado = montoPagado;
        }

        // Actualizar la lista de ventas completa solo si se solicita
        if (actualizarTabla) {
          this.obtenerVentas(this.currentPage, this.pageSize);
        }
      } catch (error) {
        console.error("Error al actualizar venta:", error);
      }
    },

    confirmarEliminarPago(pago) {
      // Verificar si la caja está cerrada para la venta seleccionada
      if (
        this.ventaSeleccionadaParaPago &&
        !this.verificarModalPermitido(this.ventaSeleccionadaParaPago)
      ) {
        return;
      }

      // Usar pagoId directamente
      if (!pago.pagoId) {
        console.error("No se pudo encontrar el ID del pago:", pago);
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "No se pudo identificar el pago a eliminar.",
          background: "#18181b",
          color: "#fff",
        });
        return;
      }

      Swal.fire({
        title: "¿Eliminar este pago?",
        text: `Método: ${pago.metodoPago}, Monto: ${this.formatoMoneda(
          pago.monto
        )}`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#dc3545",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then(async (result) => {
        if (result.isConfirmed) {
          await this.eliminarPago(pago.pagoId);
        }
      });
    },

    async eliminarPago(pagoId) {
      try {
        // Verificar si la venta está cerrada
        if (
          this.ventaSeleccionadaParaPago &&
          this.estaCajaCerrada(this.ventaSeleccionadaParaPago)
        ) {
          Swal.fire({
            icon: "error",
            title: "Operación no permitida",
            text: "No se pueden eliminar pagos porque la venta está cerrada.",
            background: "#18181b",
            color: "#fff",
            confirmButtonColor: "#dc3545",
          });
          return;
        }

        console.log("Eliminando pago con ID:", pagoId);
        await VentaService.eliminarPago(pagoId);

        // Actualizar la lista de pagos y el monto pendiente
        await this.cargarPagosExistentes(this.ventaSeleccionadaParaPago.id);

        // Actualizar el estado de la venta en la tabla inmediatamente
        // Primero actualizamos la venta seleccionada
        await this.actualizarVentaSeleccionada(false);

        // Luego actualizamos el estado en la tabla de ventas
        if (
          this.ventaSeleccionadaParaPago &&
          this.ventaSeleccionadaParaPago.id
        ) {
          // Encontrar la venta en la tabla y actualizar su estado
          const ventaIndex = this.ventas.findIndex(
            (v) => v.id === this.ventaSeleccionadaParaPago.id
          );
          if (ventaIndex !== -1) {
            // Calcular si está pagada basado en el total pagado actual
            const pagado =
              this.totalPagado >= this.ventaSeleccionadaParaPago.totalVenta;
            // Actualizar el estado en la tabla
            this.ventas[ventaIndex].estado = pagado;
            this.ventas[ventaIndex].montoPagado = this.totalPagado;
          }
        }

        Swal.fire({
          icon: "success",
          title: "Pago eliminado",
          background: "#18181b",
          color: "#fff",
          timer: 2000,
          showConfirmButton: false,
        });
      } catch (error) {
        console.error("Error al eliminar pago:", error);
        Swal.fire({
          icon: "error",
          title: "Error",
          text:
            "No se pudo eliminar el pago. Detalles: " +
            (error.response?.data?.message || error.message),
          background: "#18181b",
          color: "#fff",
        });
      }
    },

    verificarCierreSinGuardar() {
      // Solo mostrar confirmación si hay pagos pendientes
      if (this.pagosPendientes.length > 0) {
        Swal.fire({
          title: "¿Cerrar sin guardar?",
          text: "Hay pagos pendientes que no se han guardado. ¿Desea cerrar sin guardarlos?",
          icon: "warning",
          showCancelButton: true,
          confirmButtonColor: "#dc3545",
          cancelButtonColor: "#6c757d",
          confirmButtonText: "Sí, cerrar",
          cancelButtonText: "No, continuar editando",
          background: "#18181b",
          color: "#fff",
        }).then((result) => {
          if (result.isConfirmed) {
            this.cerrarModalPagos();
          }
        });
      } else {
        this.cerrarModalPagos();
      }
    },

    cerrarPagarModal() {
      this.cerrarModalPagos();
    },

    cerrarModalPagos() {
      this.mostrarPagarModal = false;
      this.ventaSeleccionadaParaPago = null;
      this.pagoForm = {
        metodo: "",
        monto: "",
        pendiente: 0,
      };
      this.montoInvalido = false;
      this.mensajeError = "";
      this.pagosRegistrados = [];
      this.pagosPendientes = [];
      this.totalPagado = 0;
    },

    // Método para mostrar confirmación de pagos con SweetAlert2
    mostrarConfirmacionPagos() {
      if (this.pagosPendientes.length === 0) {
        Swal.fire({
          icon: "info",
          title: "Sin pagos pendientes",
          text: "No hay pagos nuevos para confirmar.",
          background: "#18181b",
          color: "#fff",
        });
        return;
      }

      // Calcular el total a pagar
      const totalPendiente = this.pagosPendientes.reduce(
        (total, pago) => total + pago.monto,
        0
      );

      // Crear lista HTML de pagos pendientes
      let listaPagos = '<div class="pagos-lista-swal">';
      this.pagosPendientes.forEach((pago) => {
        listaPagos += `<div class="pago-item-swal">
          <span class="pago-metodo-swal">${pago.metodoPago}</span>
          <span class="pago-monto-swal">${this.formatoMoneda(pago.monto)}</span>
        </div>`;
      });
      listaPagos += "</div>";

      // Mensaje adicional si la venta quedará pagada
      const mensajeEstado =
        this.totalPagado >= this.ventaSeleccionadaParaPago?.totalVenta
          ? '<div class="estado-completo-swal"><i class="pi pi-check-circle"></i> La venta quedará en estado PAGADO</div>'
          : "";

      // Verificar si la caja está cerrada
      const cajaCerrada = this.estaCajaCerrada(this.ventaSeleccionadaParaPago);

      Swal.fire({
        title: "¿Confirmar los siguientes pagos?",
        html: `
          <div class="confirmacion-swal">
            <div class="info-item-swal">
              <span class="info-label-swal">Cliente:</span>
              <span class="info-value-swal">${
                this.ventaSeleccionadaParaPago?.cliente
              }</span>
            </div>
            <div class="info-item-swal">
              <span class="info-label-swal">Total venta:</span>
              <span class="info-value-swal">${this.formatoMoneda(
                this.ventaSeleccionadaParaPago?.totalVenta || 0
              )}</span>
            </div>
            <div class="info-item-swal">
              <span class="info-label-swal">Total a pagar:</span>
              <span class="info-value-swal">${this.formatoMoneda(
                totalPendiente
              )}</span>
            </div>
            ${
              cajaCerrada
                ? `
            <div class="info-item-swal">
              <span class="info-label-swal">Estado caja:</span>
              <span class="info-value-swal" style="color: #3498db;">CERRADA</span>
            </div>`
                : ""
            }
            ${listaPagos}
            ${mensajeEstado}
          </div>
        `,
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Confirmar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then((result) => {
        if (result.isConfirmed) {
          this.confirmarPagos();
        }
      });
    },

    // Calcular el total de pagos pendientes
    calcularTotalPagosPendientes() {
      return this.pagosPendientes.reduce(
        (total, pago) => total + pago.monto,
        0
      );
    },

    // Método para confirmar y registrar los pagos en la BD
    async confirmarPagos() {
      try {
        // Verificar si la caja está cerrada para la venta seleccionada
        if (
          this.ventaSeleccionadaParaPago &&
          !this.verificarModalPermitido(this.ventaSeleccionadaParaPago)
        ) {
          return;
        }

        // Mostrar indicador de carga
        Swal.fire({
          title: "Registrando pagos...",
          text: "Por favor espere",
          allowOutsideClick: false,
          allowEscapeKey: false,
          didOpen: () => {
            Swal.showLoading();
          },
          background: "#18181b",
          color: "#fff",
        });

        // Registrar cada pago pendiente en la BD
        for (const pago of this.pagosPendientes) {
          await this.registrarPago(pago);
        }

        // Actualizar la tabla principal con los cambios
        await this.actualizarVentaSeleccionada(true);

        // Mostrar mensaje de éxito
        Swal.fire({
          icon: "success",
          title: "Pagos registrados",
          text: "Los pagos han sido registrados correctamente.",
          background: "#18181b",
          color: "#fff",
          timer: 2000,
          showConfirmButton: false,
        });

        // Cerrar el modal de pagos
        this.cerrarPagarModal();
      } catch (error) {
        console.error("Error al confirmar pagos:", error);
        Swal.fire({
          icon: "error",
          title: "Error",
          text:
            "No se pudieron registrar los pagos: " +
            (error.response?.data?.message || error.message),
          background: "#18181b",
          color: "#fff",
        });
      }
    },

    // Método para cancelar los pagos
    cancelarPagos() {
      // Preguntar al usuario si está seguro de cancelar
      Swal.fire({
        title: "¿Cancelar los cambios?",
        text: "Los pagos registrados se mantendrán en el sistema, pero no se actualizará el estado de la venta.",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#dc3545",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, cancelar",
        cancelButtonText: "No, seguir editando",
        background: "#18181b",
        color: "#fff",
      }).then((result) => {
        if (result.isConfirmed) {
          this.cerrarPagarModal();
        }
      });
    },

    formatearFecha(fechaStr) {
      try {
        if (!fechaStr) return "";

        const fecha = new Date(fechaStr);
        if (isNaN(fecha.getTime())) return "";

        return fecha.toLocaleDateString("es-AR", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
          hour: "2-digit",
          minute: "2-digit",
        });
      } catch (e) {
        console.error("Error al formatear fecha:", e);
        return "";
      }
    },

    formatoMoneda(valor) {
      return new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
      }).format(valor);
    },

    // Método para obtener la fecha formateada de una cadena de fecha
    obtenerFechaFormateada(fechaStr) {
      try {
        // Si la fecha viene en formato dd/mm/yyyy (como en la tabla)
        if (fechaStr.includes("/")) {
          const partes = fechaStr.split("/");
          if (partes.length === 3) {
            // Convertir de dd/mm/yyyy a yyyy-mm-dd
            return `${partes[2]}-${partes[1].padStart(
              2,
              "0"
            )}-${partes[0].padStart(2, "0")}`;
          }
        }

        // Si es otro formato, intentar convertir normalmente
        const fecha = new Date(fechaStr);
        if (!isNaN(fecha.getTime())) {
          return fecha.toISOString().split("T")[0];
        }

        return null;
      } catch (e) {
        console.error("Error al formatear fecha:", e);
        return null;
      }
    },

    // Método para verificar si una venta está cerrada
    estaCajaCerrada(venta) {
      if (!venta) return false;
      return venta.cierreDiarioId != null;
    },

    // Método para obtener el texto del estado de la venta
    getEstadoVenta(venta) {
      // Si tiene CierreDiarioId, mostrar "Cerrado" sin importar el estado de pago
      if (this.estaCajaCerrada(venta)) {
        return "Cerrado";
      }
      // Si no, mostrar el estado normal
      return venta.estado ? "Pagado" : "Pendiente";
    },

    // Método para obtener la severidad del tag según el estado
    getEstadoSeverity(venta) {
      if (this.estaCajaCerrada(venta)) {
        return "secondary"; // Gris claro para estado "Cerrado"
      }
      return venta.estado ? "success" : "warning"; // Verde para "Pagado", amarillo para "Pendiente"
    },

    // Método para verificar si se puede abrir un modal para una venta
    verificarModalPermitido(venta) {
      if (!venta) return true;

      // Verificar si la venta está cerrada (tiene CierreDiarioId)
      if (this.estaCajaCerrada(venta)) {
        Swal.fire({
          icon: "error",
          title: "Operación no permitida",
          text: "No se puede realizar esta operación porque la venta ya está cerrada.",
          background: "#18181b",
          color: "#fff",
          confirmButtonColor: "#dc3545",
        });
        return false;
      }

      return true;
    },

    // Método para obtener el mensaje de tooltip para el botón Agregar Pago
    getAgregarPagoTooltip() {
      // Si la venta está cerrada
      if (
        this.ventaSeleccionadaParaPago &&
        this.estaCajaCerrada(this.ventaSeleccionadaParaPago)
      ) {
        return {
          value: "No se puede agregar pagos: la venta está cerrada",
          class: "tooltip-error",
        };
      }

      // Si el total ya está pagado
      if (this.totalPagado >= this.ventaSeleccionadaParaPago?.totalVenta) {
        return {
          value: "El monto total ya está pagado",
          class: "tooltip-info",
        };
      }

      // Si no se ha seleccionado método de pago
      if (!this.pagoForm.metodo) {
        return {
          value: "Seleccione un método de pago para continuar",
          class: "tooltip-warning",
        };
      }

      // Si el monto es inválido
      if (this.montoInvalido) {
        return {
          value: this.mensajeError || "El monto ingresado no es válido",
          class: "tooltip-error",
        };
      }

      // Si todo está bien, no mostrar tooltip
      return null;
    },

    eliminarProductoServicio(index) {
      // Verificar si la venta está pagada
      if (this.ventaOriginal && this.ventaOriginal.pagos.length > 0) {
        Swal.fire({
          icon: "warning",
          title: "Acción no permitida",
          text: "Debe eliminar los pagos registrados antes de eliminar productos o servicios de una venta pagada.",
          background: "#18181b",
          color: "#fff",
          confirmButtonColor: "#dc3545",
        });
        return;
      }

      // Eliminar el producto o servicio de la lista
      this.ventaSeleccionada.detalles.splice(index, 1);
    },

    async confirmarEliminarVenta(venta) {
      if (!venta.id) {
        Swal.fire({
          icon: "error",
          title: "Error",
          text: "No se encontró el ID de la venta para eliminar.",
          background: "#18181b",
          color: "#fff",
        });
        return;
      }

      Swal.fire({
        title: "¿Eliminar esta venta?",
        text: "Esta acción no se puede deshacer.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#dc3545",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then(async (result) => {
        if (result.isConfirmed) {
          try {
            await VentaService.eliminarVenta(venta.id);
            Swal.fire({
              icon: "success",
              title: "Venta eliminada",
              background: "#18181b",
              color: "#fff",
              timer: 2000,
              showConfirmButton: false,
            });
            this.obtenerVentas(this.currentPage, this.pageSize);
          } catch (error) {
            console.error("Error al eliminar venta:", error);
            Swal.fire({
              icon: "error",
              title: "Error",
              text: "No se pudo eliminar la venta.",
              background: "#18181b",
              color: "#fff",
            });
          }
        }
      });
    },
  },
};
</script>

<style scoped>
/* ===========================
   CONTENEDOR GENERAL
=========================== */
.clientes-container {
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
  width: 200px !important; /* Increased width */
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
  flex-direction: row; /* Alineación horizontal */
  gap: 0.25rem;
  justify-content: flex-start; /* Alineación izquierda */
  align-items: center;
}

.espacio-izquierda {
  width: 60px; /* Ajusta el tamaño del espacio según sea necesario */
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

.icono-check {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 30px;
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
:deep(.p-tag-info) {
  background-color: #3498db !important;
  color: #ffffff !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}

:deep(.p-tag-secondary) {
  background-color: #d3d3d3 !important; /* Gris claro */
  color: #333333 !important; /* Texto oscuro */
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
  margin-top: 1.5rem;
  font-size: 1rem;
  font-weight: 500;
  text-align: left;
  color: #aeaeae;
}

:deep(.p-card-content) {
  padding-bottom: 0.2rem !important;
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
  position: relative;
  z-index: 1;
}

.form-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: flex-end;
  margin-bottom: 0.4rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group:nth-child(1) {
  flex: 2;
}

.form-group:nth-child(2) {
  flex: 1;
}

.btn-container {
  display: flex;
  align-items: flex-end;
  justify-content: flex-end;
}

/* Estilos para el modal de pago */
:deep(.modal-pago .p-dialog-header) {
  background-color: #1a1a1a;
  color: white;
  border-bottom: 1px solid #444;
  padding: 0.75rem 1rem;
}

:deep(.modal-pago .p-dialog-content) {
  background-color: #1a1a1a;
  color: #e0e0e0;
  padding: 0.75rem 1rem;
  max-height: 80vh;
  overflow-y: auto;
}

/* Estilos para el modal compacto */
.compact-modal h4.compact-title {
  margin-top: 0;
  margin-bottom: 0.5rem;
  font-size: 1.1rem;
}

.compact-separator {
  margin: 0.5rem 0;
}

.compact-modal .form-group {
  gap: 0.3rem;
}

/* Estilos para la información de la venta */
.info-venta {
  background-color: #1e1e1e;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.info-label {
  font-size: 0.8rem;
  color: #999;
}

.info-value {
  font-size: 1rem;
  font-weight: 600;
  color: #fff;
}

.info-value.pendiente {
  color: #f0ad4e;
}

/* Separador */
.separador {
  height: 1px;
  background-color: #333;
  margin: 1rem 0;
}

/* Estilos para la lista de pagos */
.lista-pagos {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  max-height: 150px;
  overflow-y: auto;
  margin-bottom: 0.75rem;
  border: 1px solid #333;
  border-radius: 6px;
  padding: 0.4rem;
  background-color: #121212;
}

.pago-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #1e1e1e;
  border-radius: 6px;
  padding: 0.5rem 0.75rem;
  margin-bottom: 0.3rem;
  border: 1px solid #333;
}

.pago-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.pago-metodo {
  font-weight: 600;
  min-width: 100px;
}

.pago-monto {
  color: #66ff66;
  font-weight: 600;
  min-width: 80px;
}

.pago-fecha {
  color: #999;
  font-size: 0.85rem;
}

.pago-pendiente-badge {
  color: #ffc107;
  font-style: italic;
  font-weight: 600;
}

.pago-item.pago-pendiente {
  border: 1px dashed #ffc107;
  background-color: rgba(255, 193, 7, 0.1);
}

.sin-pagos {
  text-align: center;
  color: #999;
  padding: 1rem;
  font-style: italic;
}

.pagos-cargando {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: #999;
  padding: 1rem;
}

/* Botón de agregar pago */
.btn-agregar-pago {
  background-color: #28a745 !important;
  color: white !important;
  border: none !important;
  width: auto !important;
  height: auto !important;
  white-space: nowrap;
}

/* Botón de eliminar pago */
.btn-eliminar-pago {
  min-width: 30px !important;
  width: 30px !important;
  height: 30px !important;
  padding: 0 !important;
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}

/* Botones de acción en el modal */
.acciones-modal {
  display: flex;
  justify-content: space-between;
  margin-top: 0.75rem;
}

.btn-confirmar {
  background-color: #28a745 !important;
  color: white !important;
  border: none !important;
  padding: 0.5rem 1rem !important;
  width: auto !important;
  height: auto !important;
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

/* Estilos para el total pagado */
.info-total-pagado {
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
  padding: 0.5rem 0.75rem;
  background-color: #1e1e1e;
  border-radius: 6px;
  border: 1px solid #333;
}

.info-row {
  display: flex;
  flex-direction: row;
  justify-content: space-between;
  gap: 1rem;
}

.info-total-pagado .info-item {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.info-label {
  color: #999;
  font-size: 0.9rem;
  margin-bottom: 0.25rem;
}

.info-value {
  font-size: 1.1rem;
  font-weight: 600;
}

.total-completo {
  color: #28a745 !important;
  font-weight: bold;
}

.info-value.pendiente {
  color: #ffc107 !important;
  font-weight: bold;
}

.badge-completo {
  background-color: #28a745;
  color: white;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.8rem;
  margin-left: 0.5rem;
}

/* Estilos para el SweetAlert2 personalizado */
:global(.confirmacion-swal) {
  text-align: left;
  margin-top: 1rem;
  padding: 1rem;
  background-color: #1e1e1e;
  border-radius: 8px;
  border: 1px solid #333;
}

:global(.info-item-swal) {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.5rem;
  padding: 0.25rem 0;
}

:global(.info-label-swal) {
  color: #999;
  font-size: 0.9rem;
}

:global(.info-value-swal) {
  font-weight: 600;
  color: #fff;
}

:global(.pagos-lista-swal) {
  margin-top: 0.75rem;
  border-top: 1px solid #333;
  padding-top: 0.75rem;
  max-height: 150px;
  overflow-y: auto;
}

:global(.pago-item-swal) {
  display: flex;
  justify-content: space-between;
  background-color: #121212;
  padding: 0.5rem 0.75rem;
  border-radius: 4px;
  border: 1px solid #333;
  margin-bottom: 0.5rem;
}

:global(.pago-metodo-swal) {
  font-weight: 600;
}

:global(.pago-monto-swal) {
  color: #66ff66;
  font-weight: 600;
}

:global(.estado-completo-swal) {
  margin-top: 0.75rem;
  color: #28a745;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
  padding-top: 0.5rem;
  border-top: 1px solid #333;
}

/* Estilos para tooltips */
:deep(.tooltip-error) {
  background-color: #dc3545 !important;
  color: white !important;
  font-weight: 600 !important;
  padding: 0.5rem 0.75rem !important;
  border-radius: 4px !important;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3) !important;
  font-size: 0.9rem !important;
  max-width: 250px !important;
  text-align: center !important;
  z-index: 9999 !important;
}

:deep(.tooltip-warning) {
  background-color: #ffc107 !important;
  color: #212529 !important;
  font-weight: 600 !important;
  padding: 0.5rem 0.75rem !important;
  border-radius: 4px !important;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3) !important;
  font-size: 0.9rem !important;
  max-width: 250px !important;
  text-align: center !important;
  z-index: 9999 !important;
}

:deep(.tooltip-info) {
  background-color: #17a2b8 !important;
  color: white !important;
  font-weight: 600 !important;
  padding: 0.5rem 0.75rem !important;
  border-radius: 4px !important;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.3) !important;
  font-size: 0.9rem !important;
  max-width: 250px !important;
  text-align: center !important;
  z-index: 9999 !important;
}

/* Estilos para botones deshabilitados */
.button-disabled {
  opacity: 0.5 !important;
  cursor: not-allowed !important;
  background-color: rgba(0, 0, 0, 0.1) !important;
  border: 1px solid rgba(255, 255, 255, 0.1) !important;
  color: #999 !important;
  position: relative;
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

/* Centrar el header de la columna Producto y Estado */
:deep(.p-datatable-thead > tr > th:nth-child(2)),
:deep(.p-datatable-thead > tr > th:nth-child(5)) {
  text-align: center !important;
  justify-content: center !important;
  align-items: center !important;
}

/* Evitar espacio negro entre header Estado y filtro */
:deep(.p-column-header-content) {
  width: 100%;
  justify-content: center;
  align-items: center;
}

/* Ajuste para el filtro de Estado */
:deep(.p-column-filter) {
  display: flex;
  justify-content: center;
  align-items: center;
  background: transparent !important;
  padding: 0.15rem 0.3rem !important;
  margin: 0 !important;
  min-height: 40px;
}

/* Ajuste para el filtro de fecha */
:deep(.filtro-fecha .p-inputtext) {
  min-width: 110px !important;
  max-width: 140px !important;
  font-size: 0.95rem !important;
  padding: 0.2rem 0.5rem !important;
}
:deep(.p-calendar) {
  width: 140px !important;
  min-width: 120px !important;
  max-width: 160px !important;
}

/* Estilos para el dropdown en el modal de pago */
.formulario-pago :deep(.p-dropdown) {
  position: relative;
  z-index: 2;
}

/* Estilos para el indicador de campo requerido y mensaje de ayuda */
.required-indicator {
  color: #ffc107;
  font-weight: bold;
  margin-left: 4px;
}

.label-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.25rem;
}

.metodo-hint {
  color: #ffc107;
  font-size: 0.75rem;
  font-style: italic;
  margin-left: 0.5rem;
}

.p-dropdown-highlight {
  border-color: #ffc107 !important;
  box-shadow: 0 0 0 1px #ffc107 !important;
}

.formulario-pago :deep(.p-dropdown-panel) {
  position: absolute !important;
  z-index: 9999 !important;
  transform: none !important;
  margin-top: 2px !important;
}

/* Estilos para asegurar que SweetAlert2 aparezca por encima del modal */
:global(.swal-container-class) {
  z-index: 10000 !important; /* Valor mayor que cualquier otro z-index en la aplicación */
}

:global(.swal2-container) {
  z-index: 10000 !important;
}

:global(.swal2-popup) {
  z-index: 10001 !important;
}
</style>
