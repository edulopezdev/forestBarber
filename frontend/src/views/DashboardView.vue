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

    <!-- Título dinámico -->
    <h2 class="dashboard-title">
      <span v-if="isBarber">
        ¡Hola {{ user?.nombre || "Barbero" }}! Aquí están tus métricas de
        {{ nombreMes(mes) }} {{ anio }}
      </span>
      <span v-else> Facturación de {{ nombreMes(mes) }} {{ anio }} </span>
    </h2>

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

      <Card class="kpi-card objetivo-card" v-if="objetivoMensual">
        <template #title>
          <div class="card-header">
            <span>Objetivo Mensual</span>
            <small class="motivacion">¡Vamos por más! 💪</small>
          </div>
        </template>
        <template #content>
          <p class="kpi-valor objetivo-valor">
            ${{ objetivoMensual.toLocaleString() }}
          </p>
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
import { ref, onMounted, computed } from "vue";
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

const isBarber = computed(() => user.value?.rolId === 2);

// Simulamos un objetivo mensual para el barbero, idealmente vendría del backend
const objetivoMensual = ref(500000); // ejemplo $500,000

function nombreMes(mesNum) {
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

onMounted(async () => {
  const userData = sessionStorage.getItem("user");
  if (userData) {
    user.value = JSON.parse(userData);
  }

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
});
</script>

<style scoped>
.clientes-container {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  color: #e0e0e0;
}

.dashboard-title {
  font-size: 2rem;
  font-weight: 700;
  text-align: center;
  margin-bottom: 2rem;
  color: #ffffff;
  text-shadow: 0 0 10px #6d6d6daa;
}

.barber-metrics {
  display: flex;
  gap: 1.2rem;
  justify-content: center;
  flex-wrap: wrap;
}

.barber-charts {
  margin-top: 2rem;
  width: 100%;
}

.kpi-card {
  flex: 1 1 250px;
  background-color: rgba(34, 34, 34, 0.5);
  padding: 1.2rem 1rem;
  text-align: center;
  border-radius: 10px;
  box-shadow: 0 0 12px #00b894aa;
  transition: transform 0.3s ease;
}

.kpi-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 0 20px #00d27a;
}

.kpi-valor {
  font-size: 2.4rem;
  font-weight: 800;
  margin: 0.4rem 0 0;
  color: #00d27a;
  letter-spacing: 0.05em;
}

.card-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  font-weight: 700;
  font-size: 1.2rem;
  color: #a1f0c4;
  margin-bottom: 0.4rem;
}

.motivacion {
  font-weight: 400;
  font-size: 0.8rem;
  color: #55ff91;
  margin-top: 0.1rem;
  font-style: italic;
}

.objetivo-card {
  position: relative;
  background-color: #1e3a26;
  box-shadow: 0 0 12px #22d185cc;
}

.objetivo-valor {
  color: #a3e635;
}

.progress-bar {
  width: 100%;
  height: 16px;
  background: #064e3b;
  border-radius: 12px;
  margin: 0.8rem 0 0.2rem;
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
  box-shadow: 0 0 10px #4ade80;
}

.progreso-text {
  font-size: 0.85rem;
  color: #a7f3d0;
  font-weight: 600;
}

.cards-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: space-between;
  margin-bottom: 2rem;
}

.charts-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: space-between;
  flex: 1;
  overflow: hidden;
}

.chart-card {
  background-color: rgba(34, 34, 34, 0.5);
  border-radius: 10px;
  padding: 1rem;
  box-shadow: 0 0 12px #00b894aa;
  transition: transform 0.3s ease;
}
.chart-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 0 20px #00d27a;
}

.full-width-card {
  flex: 1 1 100%;
  max-width: 100%;
  width: 100%;
  padding: 1rem 0.5rem;
}

.barber-weekly-chart {
  width: 100% !important;
  height: 250px !important; /* altura menor */
}

.barber-weekly-chart canvas {
  width: 100% !important;
  height: 250px !important;
}
.admin-charts-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: space-between;
  margin-bottom: 2rem;
  width: 100%;
}

.half-width-card {
  flex: 1 1 48%;
  background-color: rgba(34, 34, 34, 0.5);
  padding: 1rem;
  border-radius: 10px;
  box-shadow: 0 0 12px #00b894aa;
  transition: transform 0.3s ease;
}

.half-width-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 0 20px #00d27a;
}

@media (max-width: 900px) {
  .barber-metrics,
  .barber-charts,
  .cards-grid,
  .charts-grid {
    flex-direction: column;
    align-items: stretch;
  }

  .kpi-card,
  .chart-card {
    min-width: 90%;
    max-width: 90%;
  }
  .admin-charts-row {
    flex-direction: column;
  }

  .half-width-card {
    min-width: 100%;
  }
}
</style>
