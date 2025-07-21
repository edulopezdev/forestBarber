<template>
  <Toast />

  <!-- Componente que muestra la notificación y oscurece el fondo -->
  <NewFeatureNotification
    featureKey="nueva-funcion-dashboard-v1"
    title="¡Nuevo módulo: Caja!"
    :duration="10000"
    top="30%"
    left="50%"
  />

  <h2 class="dashboard-title">
    Facturación de {{ nombreMes(mes) }} {{ anio }}
  </h2>

  <div class="cards-grid">
    <Card class="kpi-card total-card">
      <template #title>Total</template>
      <template #content>
        <p class="kpi-valor">
          ${{ facturacion?.facturacionTotal?.toLocaleString() }}
        </p>
      </template>
    </Card>

    <Card class="kpi-card servicio-card">
      <template #title>Servicios</template>
      <template #content>
        <p class="kpi-valor">
          ${{ facturacion?.servicios.total?.toLocaleString() }}
        </p>
      </template>
    </Card>

    <Card class="kpi-card producto-card">
      <template #title>Productos</template>
      <template #content>
        <p class="kpi-valor">
          ${{ facturacion?.productos.total?.toLocaleString() }}
        </p>
      </template>
    </Card>

    <Card class="kpi-card">
      <template #title>Total de Atenciones</template>
      <template #content>
        <p class="kpi-valor">
          {{ facturacion?.totalAtenciones?.toLocaleString() }}
        </p>
      </template>
    </Card>
  </div>

  <div class="charts-grid">
    <Card class="chart-card">
      <template #title>Métodos de Pago</template>
      <template #content>
        <Chart
          type="pie"
          :data="chartData"
          :options="chartOptions"
          style="width: 350px; height: 400px"
        />
      </template>
    </Card>

    <Card class="chart-card">
      <template #title>Cortes vs Ventas</template>
      <template #content>
        <Chart
          type="bar"
          :data="detalleChart"
          :options="barOptions"
          style="width: 350px; height: 400px"
        />
      </template>
    </Card>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import Chart from "primevue/chart";
import Card from "primevue/card";
import Toast from "primevue/toast";
import FacturacionService from "@/services/FacturacionService";
import NewFeatureTooltip from "@/components/NewFeatureNotification.vue";

const facturacion = ref(null);
const chartData = ref({});
const detalleChart = ref({});
const chartOptions = ref({});
const barOptions = ref({});

const ahora = new Date();
const anio = ref(ahora.getFullYear());
const mes = ref(ahora.getMonth() + 1); // getMonth() va de 0 a 11, por eso +1
const featureKey = "nueva-funcion-dashboard-v1";

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

onMounted(async () => {
  try {
    const response = await FacturacionService.getFacturacionPorMes(
      anio.value,
      mes.value
    );
    facturacion.value = response.data;

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
          font: {
            weight: "bold",
            size: 12,
          },
        },
      },
    };

    barOptions.value = {
      responsive: true,
      scales: {
        x: {
          ticks: { color: "#e0e0e0", font: { size: 12 } },
        },
        y: {
          ticks: { color: "#e0e0e0", font: { size: 12 } },
        },
      },
      plugins: {
        legend: {
          labels: { color: "#e0e0e0", font: { size: 12 } },
        },
      },
    };
  } catch (error) {
    console.error("Error al cargar facturación:", error);
  }
});
</script>

<style scoped>
.dashboard-container {
  padding: 1rem 1.5rem;
  background-color: #121212;
  color: #f0f0f0;
  font-family: "Segoe UI", sans-serif;
  height: 100vh; /* esto es CLAVE */
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  overflow-y: hidden; /* oculta scroll si lo hay por desborde */
}

.dashboard-title {
  font-size: 1.6rem;
  text-align: center;
  margin-bottom: 1rem;
}

.cards-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.8rem;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.kpi-card {
  flex: 1 1 200px;
  background-color: #1c1c1c;
  padding: 0.8rem;
  text-align: center;
  border-radius: 6px;
  box-shadow: 0 0 6px rgba(0, 0, 0, 0.3);
}

.kpi-valor {
  font-size: 1.4rem;
  font-weight: bold;
  margin: 0.2rem 0;
}

.total-card .kpi-valor {
  color: #27ae60;
}
.servicio-card .kpi-valor {
  color: #3498db;
}
.producto-card .kpi-valor {
  color: #f1c40f;
}

.charts-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.8rem;
  justify-content: space-between;
  flex: 1;
  overflow: hidden;
}

.chart-card {
  flex: 1 1 320px;
  background-color: #1c1c1c;
  padding: 1rem;
  height: auto;
  min-height: unset;
  border-radius: 6px;
  box-shadow: 0 0 6px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: center;
  overflow: hidden;
}

.chart-card > .p-card-title {
  margin-bottom: 0.8rem;
  width: 100%;
  text-align: center;
  font-weight: 600;
  font-size: 1.2rem;
  color: #e0e0e0;
}

.p-chart {
  width: 100% !important;
  height: 250px !important;
  max-height: 250px;
}

.p-chart canvas {
  width: 100% !important;
  height: 250px !important;
  max-height: 250px;
  box-sizing: border-box;
}

@media (max-width: 900px) {
  .cards-grid,
  .charts-grid {
    flex-direction: column;
  }

  .kpi-card,
  .chart-card {
    min-width: 100%;
  }
}
</style>
