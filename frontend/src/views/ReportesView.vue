<template>
  <div class="reportes-container">
    <Toast />
    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Reportes Diarios</h4>
          <div class="botones-acciones">
            <Button
              label="Filtros"
              icon="pi pi-filter"
              class="boton-filtros"
              @click="mostrarFiltros = !mostrarFiltros"
            />
            <Button
              label="Generar Reporte"
              icon="pi pi-file-pdf"
              class="boton-generar-reporte"
              @click="abrirModalReporte()"
            />
          </div>
        </div>
      </template>

      <template #content>
        <!-- Filtros de fecha -->
        <div v-if="mostrarFiltros" class="filtros-fecha mb-4">
          <div class="filtro-item">
            <label for="fechaConsulta">Fecha de consulta:</label>
            <Calendar
              id="fechaConsulta"
              v-model="fechaSeleccionada"
              dateFormat="dd/mm/yy"
              placeholder="Seleccionar fecha"
              showIcon
              @date-select="consultarReporte"
            />
          </div>
          <div class="filtro-item">
            <label for="tiporeporte">Tipo de reporte:</label>
            <Dropdown
              id="tiporeporte"
              v-model="tipoReporte"
              :options="tiposReporte"
              optionLabel="label"
              optionValue="value"
              placeholder="Seleccionar tipo"
              @change="consultarReporte"
            />
          </div>
        </div>

        <!-- Tarjetas de resumen -->
        <div v-if="reporteData" class="resumen-grid mb-4">
          <Card class="tarjeta-resumen">
            <template #content>
              <div class="stat-content">
                <i class="pi pi-calendar stat-icon"></i>
                <div>
                  <h3>{{ reporteData.resumen?.totalTurnos || 0 }}</h3>
                  <p>Turnos del día</p>
                </div>
              </div>
            </template>
          </Card>

          <Card class="tarjeta-resumen">
            <template #content>
              <div class="stat-content">
                <i class="pi pi-users stat-icon"></i>
                <div>
                  <h3>{{ reporteData.resumen?.totalAtenciones || 0 }}</h3>
                  <p>Atenciones realizadas</p>
                </div>
              </div>
            </template>
          </Card>

          <Card class="tarjeta-resumen">
            <template #content>
              <div class="stat-content">
                <i class="pi pi-dollar stat-icon"></i>
                <div>
                  <h3>{{ formatoMoneda(reporteData.resumen?.totalFacturado || 0) }}</h3>
                  <p>Total facturado</p>
                </div>
              </div>
            </template>
          </Card>

          <Card class="tarjeta-resumen">
            <template #content>
              <div class="stat-content">
                <i class="pi pi-credit-card stat-icon"></i>
                <div>
                  <h3>{{ formatoMoneda(reporteData.resumen?.totalPagado || 0) }}</h3>
                  <p>Total cobrado</p>
                </div>
              </div>
            </template>
          </Card>
        </div>

        <!-- Tabla de atenciones -->
        <DataTable
          v-if="reporteData?.atenciones"
          :value="reporteData.atenciones"
          paginator
          :rows="10"
          :loading="loading"
          tableStyle="min-width: 100%"
          class="tabla-atenciones"
        >
          <Column field="cliente.nombre" header="Cliente" sortable>
            <template #body="slotProps">
              {{ slotProps.data.cliente?.nombre || 'Sin cliente' }}
            </template>
          </Column>

          <Column field="barbero.nombre" header="Barbero" sortable>
            <template #body="slotProps">
              {{ slotProps.data.barbero?.nombre || 'Sin barbero' }}
            </template>
          </Column>

          <Column field="fecha" header="Hora" sortable>
            <template #body="slotProps">
              {{ formatearHora(slotProps.data.fecha) }}
            </template>
          </Column>

          <Column header="Servicios">
            <template #body="slotProps">
              <div class="servicios-lista">
                <Tag
                  v-for="servicio in slotProps.data.servicios"
                  :key="servicio.producto"
                  :value="servicio.producto"
                  class="servicio-tag"
                />
              </div>
            </template>
          </Column>

          <Column field="totalAtencion" header="Total" sortable>
            <template #body="slotProps">
              {{ formatoMoneda(slotProps.data.totalAtencion) }}
            </template>
          </Column>

          <Column header="Estado Pago">
            <template #body="slotProps">
              <Tag
                :value="slotProps.data.pagos?.length > 0 ? 'Pagado' : 'Pendiente'"
                :severity="slotProps.data.pagos?.length > 0 ? 'success' : 'warning'"
              />
            </template>
          </Column>

          <Column header="Acciones" style="min-width: 120px">
            <template #body="slotProps">
              <div class="acciones-botones">
                <Button
                  icon="pi pi-eye"
                  severity="info"
                  text
                  rounded
                  v-tooltip.bottom="'Ver detalles'"
                  @click="verDetalleAtencion(slotProps.data)"
                />
                <Button
                  icon="pi pi-print"
                  severity="secondary"
                  text
                  rounded
                  v-tooltip.bottom="'Imprimir'"
                  @click="imprimirAtencion(slotProps.data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>

        <!-- Mensaje cuando no hay datos -->
        <div v-if="!reporteData && !loading" class="no-datos">
          <i class="pi pi-info-circle"></i>
          <p>Selecciona una fecha para generar el reporte</p>
        </div>

        <div v-if="reporteData && reporteData.atenciones?.length === 0" class="no-datos">
          <i class="pi pi-exclamation-circle"></i>
          <p>No hay atenciones registradas para la fecha seleccionada</p>
        </div>
      </template>
    </Card>

    <!-- Modal para configurar reporte -->
    <Dialog
      v-model:visible="mostrarModalReporte"
      header="Configurar Reporte"
      :modal="true"
      :closable="false"
      style="width: 500px"
    >
      <ReportesForm
        :fecha="fechaSeleccionada"
        @generar="generarReporte"
        @cancelar="cerrarModalReporte"
      />
    </Dialog>

    <!-- Modal detalle atencion -->
    <Dialog
      v-model:visible="mostrarDetalleModal"
      header="Detalle de Atención"
      :modal="true"
      :closable="false"
      style="width: 600px"
    >
      <div v-if="atencionSeleccionada" class="detalle-atencion">
        <div class="campo">
          <label>Cliente:</label>
          <p>{{ atencionSeleccionada.cliente?.nombre }}</p>
        </div>
        <div class="campo">
          <label>Barbero:</label>
          <p>{{ atencionSeleccionada.barbero?.nombre }}</p>
        </div>
        <div class="campo">
          <label>Fecha y hora:</label>
          <p>{{ formatearFechaCompleta(atencionSeleccionada.fecha) }}</p>
        </div>
        <div class="campo">
          <label>Servicios:</label>
          <div class="servicios-detalle">
            <div
              v-for="servicio in atencionSeleccionada.servicios"
              :key="servicio.producto"
              class="servicio-item"
            >
              <span>{{ servicio.producto }} x{{ servicio.cantidad }}</span>
              <span>{{ formatoMoneda(servicio.subtotal) }}</span>
            </div>
          </div>
        </div>
        <div class="campo">
          <label>Total:</label>
          <p class="total-destacado">{{ formatoMoneda(atencionSeleccionada.totalAtencion) }}</p>
        </div>
        <div class="campo">
          <label>Pagos:</label>
          <div v-if="atencionSeleccionada.pagos?.length > 0" class="pagos-detalle">
            <div
              v-for="pago in atencionSeleccionada.pagos"
              :key="pago.metodo + pago.fecha"
              class="pago-item"
            >
              <span>{{ pago.metodo }}</span>
              <span>{{ formatoMoneda(pago.monto) }}</span>
            </div>
          </div>
          <p v-else class="text-muted">Sin pagos registrados</p>
        </div>
      </div>
      <div class="acciones-modal">
        <Button
          label="Cerrar"
          icon="pi pi-times"
          @click="mostrarDetalleModal = false"
        />
      </div>
    </Dialog>
  </div>
</template>

<script>
import ReportesService from "../services/ReportesService";
import UsuarioService from "../services/UsuarioService";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Card from "primevue/card";
import Tag from "primevue/tag";
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Dropdown from "primevue/dropdown";
import Dialog from "primevue/dialog";
import Toast from "primevue/toast";
import Swal from "sweetalert2";
import ReportesForm from "../components/ReportesForm.vue";

export default {
  name: "ReportesView",
  components: {
    DataTable,
    Column,
    Card,
    Tag,
    Button,
    Calendar,
    Dropdown,
    Dialog,
    Toast,
    ReportesForm,
  },
  data() {
    return {
      loading: false,
      mostrarFiltros: true,
      mostrarModalReporte: false,
      mostrarDetalleModal: false,
      fechaSeleccionada: new Date(),
      tipoReporte: 'completo',
      reporteData: null,
      atencionSeleccionada: null,
      barberos: [],
      
      tiposReporte: [
        { label: 'Reporte Completo', value: 'completo' },
        { label: 'Solo Turnos', value: 'turnos' },
        { label: 'Solo Atenciones', value: 'atenciones' },
        { label: 'Solo Pagos', value: 'pagos' },
      ],
    };
  },
  mounted() {
    this.cargarBarberos();
    this.consultarReporte();
  },
  methods: {
    async cargarBarberos() {
      try {
        const response = await UsuarioService.getUsuarios(1, 100, { rolNombre: 'Barbero' });
        this.barberos = response.data.usuarios || [];
      } catch (error) {
        console.error('Error al cargar barberos:', error);
      }
    },

    async consultarReporte() {
      if (!this.fechaSeleccionada) return;

      this.loading = true;
      try {
        let response;
        
        switch (this.tipoReporte) {
          case 'turnos':
            response = await ReportesService.getTurnosPorDia(this.fechaSeleccionada);
            break;
          case 'atenciones':
            response = await ReportesService.getAtencionesPorDia(this.fechaSeleccionada);
            break;
          case 'pagos':
            response = await ReportesService.getPagosPorDia(this.fechaSeleccionada);
            break;
          default:
            response = await ReportesService.getReporteDiaCompleto(this.fechaSeleccionada);
        }

        this.reporteData = response.data;
      } catch (error) {
        console.error('Error al consultar reporte:', error);
        Swal.fire('Error', 'No se pudo generar el reporte.', 'error');
      } finally {
        this.loading = false;
      }
    },

    abrirModalReporte() {
      this.mostrarModalReporte = true;
    },

    cerrarModalReporte() {
      this.mostrarModalReporte = false;
    },

    generarReporte(configuracion) {
      console.log('Generar reporte con configuración:', configuracion);
      this.cerrarModalReporte();
      // Aquí puedes implementar la lógica para generar PDF, Excel, etc.
    },

    verDetalleAtencion(atencion) {
      this.atencionSeleccionada = atencion;
      this.mostrarDetalleModal = true;
    },

    imprimirAtencion(atencion) {
      // Implementar lógica de impresión
      console.log('Imprimir atención:', atencion);
    },

    formatoMoneda(valor) {
      return new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
      }).format(valor || 0);
    },

    formatearHora(fecha) {
      return new Date(fecha).toLocaleTimeString('es-ES', { 
        hour: '2-digit', 
        minute: '2-digit' 
      });
    },

    formatearFechaCompleta(fecha) {
      return new Date(fecha).toLocaleString('es-ES');
    },
  },
};
</script>

<style scoped>
/* ===========================
   CONTENEDOR GENERAL
=========================== */
.reportes-container {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  color: #e0e0e0;
}

/* ===========================
   ENCABEZADO Y BOTONES
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

.boton-generar-reporte {
  background-color: #28a745;
  color: white;
  font-weight: normal;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.7rem;
}

/* ===========================
   FILTROS
=========================== */
.filtros-fecha {
  display: flex;
  gap: 2rem;
  padding: 1rem;
  background-color: #1a1a1a;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.filtro-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filtro-item label {
  font-weight: 600;
  color: #bbbbbb;
  font-size: 0.9rem;
}

/* ===========================
   TARJETAS DE RESUMEN
=========================== */
.resumen-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.tarjeta-resumen {
  background-color: #1a1a1a !important;
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  font-size: 2rem;
  color: #28a745;
}

.stat-content h3 {
  margin: 0;
  color: #ffffff;
  font-size: 1.5rem;
  font-weight: 600;
}

.stat-content p {
  margin: 0;
  color: #bbbbbb;
  font-size: 0.9rem;
}

/* ===========================
   TABLA DE ATENCIONES
=========================== */
.tabla-atenciones :deep(.p-datatable) {
  background-color: #121212;
  color: #eee;
  border-radius: 10px;
  border: none;
  box-shadow: 0 3px 8px rgb(0 0 0 / 0.5);
}

.tabla-atenciones :deep(.p-datatable-thead > tr > th) {
  background-color: #2a2a2a;
  color: #ffffff;
  font-weight: 600;
  text-align: center !important;
}

.tabla-atenciones :deep(.p-datatable-tbody > tr:hover) {
  background-color: #1e3a1e;
}

.servicios-lista {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.servicio-tag {
  font-size: 0.75rem !important;
  padding: 0.1rem 0.4rem !important;
}

.acciones-botones {
  display: flex;
  gap: 0.25rem;
  justify-content: center;
  align-items: center;
}

/* ===========================
   MODAL DETALLE
=========================== */
.detalle-atencion {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.campo {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.campo label {
  font-weight: 600;
  color: #bbbbbb;
  font-size: 0.9rem;
}

.campo p {
  margin: 0;
  color: #ffffff;
}

.servicios-detalle,
.pagos-detalle {
  background-color: #1a1a1a;
  border-radius: 6px;
  padding: 0.5rem;
}

.servicio-item,
.pago-item {
  display: flex;
  justify-content: space-between;
  padding: 0.25rem 0;
  border-bottom: 1px solid #333;
}

.servicio-item:last-child,
.pago-item:last-child {
  border-bottom: none;
}

.total-destacado {
  font-size: 1.2rem !important;
  font-weight: 600 !important;
  color: #28a745 !important;
}

.text-muted {
  color: #888 !important;
  font-style: italic;
}

.acciones-modal {
  display: flex;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

/* ===========================
   MENSAJE SIN DATOS
=========================== */
.no-datos {
  text-align: center;
  padding: 2rem;
  color: #888;
}

.no-datos i {
  font-size: 3rem;
  margin-bottom: 1rem;
  display: block;
}
</style>