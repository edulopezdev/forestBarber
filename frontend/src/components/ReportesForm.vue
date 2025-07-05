<template>
  <div class="reportes-form">
    <h3>Configurar Reporte</h3>

    <!-- Tipo de reporte -->
    <div class="campo">
      <label for="tipoReporte">Tipo de reporte <span class="obligatorio">*</span></label>
      <Dropdown
        id="tipoReporte"
        v-model="form.tipoReporte"
        :options="tiposReporte"
        optionLabel="label"
        optionValue="value"
        placeholder="Seleccionar tipo de reporte"
      />
    </div>

    <!-- Rango de fechas -->
    <div class="campo">
      <label for="fechaInicio">Fecha inicio <span class="obligatorio">*</span></label>
      <Calendar
        id="fechaInicio"
        v-model="form.fechaInicio"
        dateFormat="dd/mm/yy"
        placeholder="Fecha de inicio"
        showIcon
      />
    </div>

    <div class="campo">
      <label for="fechaFin">Fecha fin</label>
      <Calendar
        id="fechaFin"
        v-model="form.fechaFin"
        dateFormat="dd/mm/yy"
        placeholder="Fecha de fin (opcional)"
        showIcon
      />
    </div>

    <!-- Filtros adicionales -->
    <div class="campo" v-if="form.tipoReporte === 'barbero'">
      <label for="barbero">Barbero específico</label>
      <Dropdown
        id="barbero"
        v-model="form.barberoId"
        :options="barberos"
        optionLabel="nombre"
        optionValue="id"
        placeholder="Seleccionar barbero"
        showClear
      />
    </div>

    <!-- Formato de exportación -->
    <div class="campo">
      <label for="formato">Formato de exportación</label>
      <Dropdown
        id="formato"
        v-model="form.formato"
        :options="formatosExportacion"
        optionLabel="label"
        optionValue="value"
        placeholder="Seleccionar formato"
      />
    </div>

    <!-- Opciones adicionales -->
    <div class="campo">
      <label>Opciones adicionales</label>
      <div class="opciones-checkbox">
        <div class="checkbox-item">
          <Checkbox id="incluirGraficos" v-model="form.incluirGraficos" :binary="true" />
          <label for="incluirGraficos">Incluir gráficos</label>
        </div>
        <div class="checkbox-item">
          <Checkbox id="incluirDetalles" v-model="form.incluirDetalles" :binary="true" />
          <label for="incluirDetalles">Incluir detalles completos</label>
        </div>
        <div class="checkbox-item">
          <Checkbox id="enviarEmail" v-model="form.enviarEmail" :binary="true" />
          <label for="enviarEmail">Enviar por email</label>
        </div>
      </div>
    </div>

    <!-- Email (si está seleccionado) -->
    <div class="campo" v-if="form.enviarEmail">
      <label for="email">Email de destino</label>
      <InputText
        id="email"
        v-model="form.email"
        placeholder="email@ejemplo.com"
        type="email"
      />
    </div>

    <!-- Botones de acción -->
    <div class="acciones-formulario">
      <Button
        label="Cancelar"
        icon="pi pi-times"
        severity="secondary"
        @click="$emit('cancelar')"
      />
      <Button
        label="Generar Reporte"
        icon="pi pi-file-pdf"
        @click="generarReporte"
        :disabled="!form.tipoReporte || !form.fechaInicio"
      />
    </div>
  </div>
</template>

<script>
import Dropdown from "primevue/dropdown";
import Calendar from "primevue/calendar";
import Checkbox from "primevue/checkbox";
import InputText from "primevue/inputtext";
import Button from "primevue/button";
import UsuarioService from "../services/UsuarioService";
import ReportesService from "../services/ReportesService";

export default {
  name: "ReportesForm",
  components: {
    Dropdown,
    Calendar,
    Checkbox,
    InputText,
    Button,
  },
  props: {
    fecha: {
      type: Date,
      default: () => new Date(),
    },
    barberos: {
      type: Array,
      default: () => [],
    },
  },
  emits: ['generar', 'cancelar'],
  data() {
    return {
      form: {
        tipoReporte: 'completo',
        fechaInicio: this.fecha,
        fechaFin: null,
        barberoId: null,
        formato: 'pdf',
        incluirGraficos: true,
        incluirDetalles: false,
        enviarEmail: false,
        email: '',
      },
      tiposReporte: [
        { label: 'Reporte Completo del Día', value: 'completo' },
        { label: 'Solo Turnos', value: 'turnos' },
        { label: 'Solo Atenciones', value: 'atenciones' },
        { label: 'Solo Pagos', value: 'pagos' },
        { label: 'Por Barbero', value: 'barbero' },
        { label: 'Rango de Fechas', value: 'rango' },
      ],
      formatosExportacion: [
        { label: 'PDF', value: 'pdf' },
        { label: 'Excel', value: 'excel' },
        { label: 'CSV', value: 'csv' },
      ],
    };
  },
  methods: {
    generarReporte() {
      // Validaciones básicas
      if (!this.form.tipoReporte || !this.form.fechaInicio) {
        return;
      }

      if (this.form.enviarEmail && !this.form.email) {
        alert('Debe proporcionar un email de destino');
        return;
      }

      // Emitir configuración del reporte
      this.$emit('generar', { ...this.form });
    },
  },
};
</script>

<style scoped>
.reportes-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  color: #e0e0e0;
}

.campo {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.campo label {
  font-weight: 600;
  color: #bbbbbb;
  font-size: 0.9rem;
}

.obligatorio {
  color: #ff6b6b;
}

.opciones-checkbox {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.checkbox-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.checkbox-item label {
  margin: 0;
  font-weight: normal;
  font-size: 0.9rem;
}

.acciones-formulario {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1.5rem;
}
</style>