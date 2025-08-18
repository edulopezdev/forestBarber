<template>
  <div class="clientes-container">
    <Toast />

    <NewFeatureNotification
      featureKey="nueva-funcion-dashboard-v1"
      title="¡Nuevo módulo: Caja!"
      :duration="10000"
      top="30%"
      left="50%"
    />

    <!-- Título y filtros en la misma línea -->
    <div class="dashboard-title-row">
      <h2 class="dashboard-title">
        <span v-if="isBarber">
          ¡Hola {{ user?.nombre || "Barbero" }}!
        </span>
        <span v-else> Facturación de {{ nombreMes(mes) }} {{ anio }} </span>
      </h2>
      <div class="dashboard-filters">
        <select v-model="mes" @change="cargarFacturacion" class="dashboard-filter-select">
          <option v-for="(nombre, idx) in meses" :key="idx" :value="idx + 1">
            {{ nombre }}
          </option>
        </select>
        <select v-model="anio" @change="cargarFacturacion" class="dashboard-filter-select">
          <option v-for="a in aniosDisponibles" :key="a" :value="a">{{ a }}</option>
        </select>
      </div>
    </div>

    <!-- Vista para Barberos -->
    <div v-if="isBarber" class="barber-metrics">
      <Card class="kpi-card total-card">
        <template #title>
          <div class="card-header">
            <span>Ganancias</span>
            <small class="motivacion">¡A romperla este mes!</small>
          </div>
        </template>
        <template #content>
          <p class="kpi-valor">
            ${{ facturacion?.facturacionTotal?.toLocaleString() || "0" }}
          </p>
        </template>
      </Card>

      <Card class="kpi-card atendencias-card">
        <template #title>
          <div class="card-header">
            <span>Total de Atenciones</span>
            <small class="motivacion">Cada corte cuenta ✂️</small>
          </div>
        </template>
        <template #content>
          <p class="kpi-valor">
            {{ facturacion?.totalAtenciones?.toLocaleString() || "0" }}
          </p>
        </template>
      </Card>

      <Card class="kpi-card objetivo-card" v-if="objetivoMensual !== null">
        <template #title>
          <div class="card-header">
            <span>Objetivo Mensual</span>
            <small class="motivacion">¡Vamos por más! 💪</small>
          </div>
        </template>
        <template #content>
          <div class="objetivo-input-row">
            <span class="objetivo-label">$</span>
            <input
              type="text"
              inputmode="numeric"
              pattern="\d*"
              :value="objetivoMensual.toLocaleString('en-US')"
              @input="onObjetivoInput"
              class="objetivo-input"
            />
          </div>
          <div class="progress-bar">
            <div
              class="progress-fill"
              :style="{ width: progresoObjetivo + '%' }"
              :class="{ complete: progresoObjetivo >= 100 }"
            ></div>
          </div>
          <small class="progreso-text">
            {{ progresoObjetivo.toFixed(1) }}% del objetivo alcanzado
          </small>
        </template>
      </Card>
    </div>

    <!-- Gráfico semanal, ancho completo bajo las 3 cards -->
    <div v-if="isBarber" class="barber-charts">
      <Card class="chart-card full-width-card">
        <template #title> Ganancias Semanales </template>
        <template #content>
          <Chart
            type="bar"
            :data="barberoChartData"
            :options="barOptions"
            class="barber-weekly-chart"
            style="height: 250px; width: 100%"
          />
        </template>
      </Card>
    </div>

    <!-- Vista para Administradores y otros roles -->
    <div v-else>
      <div class="cards-grid">
        <Card class="kpi-card total-card">
          <template #title>Total</template>
          <template #content>
            <p class="kpi-valor">
              ${{ facturacion?.facturacionTotal?.toLocaleString() || "0" }}
            </p>
          </template>
        </Card>
        <Card class="kpi-card servicio-card">
          <template #title>Servicios</template>
          <template #content>
            <p class="kpi-valor">
              ${{ facturacion?.servicios.total?.toLocaleString() || "0" }}
            </p>
          </template>
        </Card>
        <Card class="kpi-card producto-card">
          <template #title>Productos</template>
          <template #content>
            <p class="kpi-valor">
              ${{ facturacion?.productos.total?.toLocaleString() || "0" }}
            </p>
          </template>
        </Card>
        <Card class="kpi-card">
          <template #title>Total de Atenciones</template>
          <template #content>
            <p class="kpi-valor">
              {{ facturacion?.totalAtenciones?.toLocaleString() || "0" }}
            </p>
          </template>
        </Card>
      </div>
      <div class="admin-charts-row">
        <Card class="chart-card half-width-card">
          <template #title>Métodos de Pago</template>
          <template #content>
            <Chart
              type="pie"
              :data="chartData"
              :options="chartOptions"
              class="admin-chart"
              style="height: 350px; width: 100%"
            />
          </template>
        </Card>
        <Card class="chart-card half-width-card">
          <template #title>Cortes vs Ventas</template>
          <template #content>
            <Chart
              type="bar"
              :data="detalleChart"
              :options="barOptions"
              class="admin-chart"
              style="height: 350px; width: 100%"
            />
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import Chart from "primevue/chart";
import Card from "primevue/card";
import Toast from "primevue/toast";
import FacturacionService from "@/services/FacturacionService";
import NewFeatureNotification from "@/components/NewFeatureNotification.vue";

const facturacion = ref(null);
const chartData = ref({});
const detalleChart = ref({});
const chartOptions = ref({});
const barOptions = ref({});
const user = ref(null);

const ahora = new Date();
const anio = ref(ahora.getFullYear());
const mes = ref(ahora.getMonth() + 1);

const meses = [
  "Enero",
  "Febrero",
  "Marzo",
  "Abril",
  "Mayo",
  "Junio",
  "Julio",
  "Agosto",
  "Septiembre",
  "Octubre",
  "Noviembre",
  "Diciembre",
];

// Años disponibles para el filtro (puedes ajustar el rango según necesidad)
const aniosDisponibles = Array.from({ length: 6 }, (_, i) => ahora.getFullYear() - i);

const isBarber = computed(() => user.value?.rolId === 2);

const OBJETIVO_KEY = "objetivoMensualBarbero";
const objetivoMensual = ref(500000); // editable por el usuario

// Cargar objetivo desde localStorage si existe
onMounted(async () => {
  const userData = sessionStorage.getItem("user");
  if (userData) {
    user.value = JSON.parse(userData);
  }
  const guardado = localStorage.getItem(OBJETIVO_KEY);
  if (guardado && !isNaN(Number(guardado))) {
    objetivoMensual.value = Number(guardado);
  }
  await cargarFacturacion();
});

// Guardar objetivo en localStorage cada vez que cambia
watch(objetivoMensual, (nuevo) => {
  if (nuevo && !isNaN(nuevo)) {
    localStorage.setItem(OBJETIVO_KEY, String(nuevo));
  }
});

function onObjetivoInput(e) {
  // Remueve todo lo que no sea número y actualiza el valor
  const raw = e.target.value.replace(/[^\d]/g, "");
  objetivoMensual.value = raw ? parseInt(raw, 10) : 0;
}

function nombreMes(mesNum) {
  return meses[mesNum - 1] || "";
}

// Cálculo de progreso hacia el objetivo
const progresoObjetivo = computed(() => {
  if (!facturacion.value?.facturacionTotal || !objetivoMensual.value) return 0;
  return (facturacion.value.facturacionTotal / objetivoMensual.value) * 100;
});

// Datos para gráfico semanal barbero (ejemplo estático, puedes traer de backend)
const barberoChartData = ref({
  labels: ["Semana 1", "Semana 2", "Semana 3", "Semana 4"],
  datasets: [
    {
      label: "Ganancias",
      backgroundColor: "#00d27a",
      data: [120000, 150000, 130000, 100000],
      barThickness: 30,
      borderRadius: 5,
    },
  ],
});

barOptions.value = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    x: {
      ticks: {
        color: "#e0e0e0",
        font: { size: 14, weight: "bold" },
      },
      grid: {
        color: "#333",
      },
    },
    y: {
      beginAtZero: true,
      ticks: {
        color: "#e0e0e0",
        font: { size: 14 },
        callback: (value) => `$${value.toLocaleString()}`,
      },
      grid: {
        color: "#2c2c2c",
      },
    },
  },
  plugins: {
    legend: {
      labels: {
        color: "#00d27a",
        font: { size: 14, weight: "bold" },
      },
    },
    tooltip: {
      callbacks: {
        label: function (context) {
          return ` $${context.raw.toLocaleString()}`;
        },
      },
    },
  },
};

// Nueva función para cargar facturación según filtros
async function cargarFacturacion() {
  try {
    const usuarioId = isBarber.value ? user.value.id : null;
    const response = await FacturacionService.getFacturacionPorMes(
      anio.value,
      mes.value,
      usuarioId
    );
    facturacion.value = response.data;

    if (facturacion.value && !isBarber.value) {
      chartData.value = {
        labels: facturacion.value.metodoPago.map((m) => m.metodo),
        datasets: [
          {
            data: facturacion.value.metodoPago.map((m) => m.total),
            backgroundColor: ["#42A5F5", "#66BB6A", "#FFA726", "#AB47BC"],
          },
        ],
      };

      detalleChart.value = {
        labels: ["Cortes", "Ventas"],
        datasets: [
          {
            label: "Monto",
            backgroundColor: ["#FF6384", "#36A2EB"],
            data: [
              facturacion.value.conclusion.totalEnCortes,
              facturacion.value.conclusion.totalEnVentas,
            ],
          },
        ],
      };

      chartOptions.value = {
        responsive: true,
        plugins: {
          legend: {
            position: "right",
            labels: {
              color: "#e0e0e0",
              font: { size: 14 },
              padding: 12,
              boxWidth: 12,
              usePointStyle: true,
            },
          },
          datalabels: {
            color: "#fff",
            formatter: (value, context) => {
              const label = context.chart.data.labels[context.dataIndex];
              return label + "\n$" + value.toLocaleString();
            },
            font: { weight: "bold", size: 12 },
          },
        },
      };
    }
  } catch (error) {
    console.error("Error al cargar facturación:", error);
  }
}

onMounted(async () => {
  const userData = sessionStorage.getItem("user");
  if (userData) {
    user.value = JSON.parse(userData);
  }
  const guardado = localStorage.getItem(OBJETIVO_KEY);
  if (guardado && !isNaN(Number(guardado))) {
    objetivoMensual.value = Number(guardado);
  }
  await cargarFacturacion();
});

// Si quieres que los filtros reactiven la carga automáticamente (además del @change)
watch([mes, anio], () => {
  // cargarFacturacion(); // Si prefieres solo con @change, puedes comentar esta línea
});
</script>

<style scoped>
.clientes-container {
  padding: 0.7rem;
  display: flex;
  flex-direction: column;
  gap: 1.1rem;
  color: #e0e0e0;
  min-height: 0;
}

.dashboard-title-row {
  display: flex;
  align-items: flex-end; /* Cambia de flex-start a flex-end para alinear abajo */
  justify-content: flex-start;
  gap: 0.7rem;
  margin-bottom: 0.1rem;
  flex-wrap: wrap;
  width: 100%;
  max-width: 100%;
}

.dashboard-title {
  font-size: 1.3rem;
  font-weight: 700;
  text-align: left;
  margin-bottom: 0;
  color: #ffffff;
  text-shadow: 0 0 7px #6d6d6daa;
  flex: 1 1 0;
  min-width: 160px;
  white-space: nowrap;
  /* Alinea el título con el borde izquierdo de la tarjeta de semanas */
  padding-left: 0.2rem;
}

.dashboard-filters {
  display: flex;
  gap: 0.5rem;
  z-index: 10;
  align-items: flex-end; /* Cambia de center a flex-end para alinear abajo */
  margin-bottom: 0.15rem; /* Agrega un pequeño margen inferior para bajarlos más */
  flex: 0 1 auto;
}

.dashboard-filter-select {
  background: rgba(34, 34, 34, 0.7);
  color: #a1f0c4;
  border: 1px solid #22d185;
  border-radius: 7px;
  padding: 0.2rem 0.7rem 0.2rem 0.5rem;
  font-size: 0.95rem;
  font-weight: 600;
  outline: none;
  transition: border 0.2s;
  box-shadow: 0 0 4px #00b89433;
  appearance: none;
  cursor: pointer;
  min-width: 90px;
}

.dashboard-filter-select:focus {
  border: 1.5px solid #4ade80;
  background: rgba(34, 34, 34, 0.95);
}

@media (max-width: 900px) {
  .dashboard-title-row {
    flex-direction: column;
    align-items: stretch;
    gap: 0.5rem;
  }
  .dashboard-title {
    text-align: center;
    margin-bottom: 0.5rem;
    white-space: normal;
    padding-left: 0;
  }
  .dashboard-filters {
    justify-content: flex-end;
    margin-bottom: 1.2rem;
  }
  .barber-metrics,
  .barber-charts,
  .cards-grid,
  .charts-grid {
    flex-direction: column;
    align-items: stretch;
  }
  .kpi-card,
  .chart-card {
    min-width: 95%;
    max-width: 95%;
  }
  .admin-charts-row {
    flex-direction: column;
  }
  .half-width-card {
    min-width: 100%;
  }
}
.barber-metrics {
  display: flex;
  gap: 0.7rem;
  justify-content: center;
  flex-wrap: nowrap;
  width: 100%;
}

.kpi-card {
  flex: 1 1 0;
  min-width: 0;
  max-width: none;
  background-color: rgba(34, 34, 34, 0.5);
  padding: 0.7rem 0.5rem;
  text-align: center;
  border-radius: 10px;
  box-shadow: 0 0 8px #00b894aa;
  transition: transform 0.3s ease;
  /* Elimina min-width y max-width previos */
}

.kpi-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 0 14px #00d27a;
}

.barber-charts {
  margin-top: 0.7rem; /* antes 1rem, ahora igual al gap de .barber-metrics */
  width: 100%;
}

.kpi-valor {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0.2rem 0 0;
  color: #00d27a;
  letter-spacing: 0.05em;
}

.card-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  font-weight: 700;
  font-size: 1rem;
  color: #a1f0c4;
  margin-bottom: 0.2rem;
}

.motivacion {
  font-weight: 400;
  font-size: 0.7rem;
  color: #55ff91;
  margin-top: 0.1rem;
  font-style: italic;
}

.objetivo-card {
  position: relative;
  background-color: #1e3a26;
  box-shadow: 0 0 8px #22d185cc;
}

.objetivo-input-row {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.2rem;
  margin-bottom: 0.2rem;
}

.objetivo-label {
  color: #a3e635;
  font-size: 1.1rem;
  font-weight: 700;
}

.objetivo-input {
  width: 90px;
  padding: 0.15rem 0.4rem;
  border-radius: 6px;
  border: 1px solid #22d185;
  background: #222c22;
  color: #a3e635;
  font-size: 1.1rem;
  font-weight: 700;
  outline: none;
  text-align: right;
  transition: border 0.2s;
}

.objetivo-input:focus {
  border: 1.5px solid #4ade80;
  background: #1e3a26;
  color: #d9f99d;
}

.progress-bar {
  width: 100%;
  height: 10px;
  background: #064e3b;
  border-radius: 12px;
  margin: 0.5rem 0 0.1rem;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: #22d185;
  transition: width 0.5s ease;
  border-radius: 12px 0 0 12px;
}

.progress-fill.complete {
  background: #4ade80;
  box-shadow: 0 0 7px #4ade80;
}

.progreso-text {
  font-size: 0.7rem;
  color: #a7f3d0;
  font-weight: 600;
}

.cards-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.7rem;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.charts-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.7rem;
  justify-content: space-between;
  flex: 1;
  overflow: hidden;
}

.chart-card {
  background-color: rgba(34, 34, 34, 0.5);
  border-radius: 10px;
  padding: 0.5rem;
  box-shadow: 0 0 8px #00b894aa;
  transition: transform 0.3s ease;
}
.chart-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 0 14px #00d27a;
}

.full-width-card {
  flex: 1 1 100%;
  max-width: 100%;
  width: 100%;
  padding: 0.5rem 0.2rem;
}

.barber-weekly-chart {
  width: 100% !important;
  height: 140px !important; /* antes 160px, ahora un poco menos */
}

.barber-weekly-chart canvas {
  width: 100% !important;
  height: 140px !important;
}
.admin-charts-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.7rem;
  justify-content: space-between;
  margin-bottom: 1rem;
  width: 100%;
}

.half-width-card {
  flex: 1 1 49.5%;
  background-color: rgba(34, 34, 34, 0.5);
  padding: 0.5rem;
  border-radius: 10px;
  box-shadow: 0 0 8px #00b894aa;
  transition: transform 0.3s ease;
  min-width: 220px;
  /* max-width: 400px; */ /* Elimina el max-width para permitir mayor ancho */
  margin-left: 0;
  margin-right: 0;
}

.half-width-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 0 14px #00d27a;
}

.admin-chart {
  height: 180px !important;
  min-height: 120px !important;
  max-height: 220px !important;
}
</style>

