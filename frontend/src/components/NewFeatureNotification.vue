<template>
  <div v-if="visible" class="overlay">
    <div class="tooltip" :style="tooltipStyle">
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
      <div class="spinner">
        <svg viewBox="0 0 50 50">
          <circle
            class="path"
            cx="25"
            cy="25"
            r="20"
            fill="none"
            stroke-width="5"
            :style="{ strokeDashoffset: strokeDashoffset }"
          />
        </svg>
      </div>
      <button class="btn-confirm" @click="close">¡Entendido!</button>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    featureKey: { type: String, required: true },
    title: { type: String, default: "¡Nueva Función Disponible!" },
    message: {
      type: String,
      default:
        'Ahora podés acceder a Caja, el espacio donde realizás tus cierres diarios de manera rápida, ordenada y sin complicaciones.',
    },
    duration: { type: Number, default: 30000 }, // duración ajustada a 30 segundos
    top: { type: String, default: "30%" },
    left: { type: String, default: "50%" },
  },
  data() {
    return {
      visible: false,
      startTime: null,
      strokeDashoffset: 125.6,
    };
  },
  computed: {
    tooltipStyle() {
      return {
        top: this.top,
        left: this.left,
        transform: "translate(-50%, -50%)",
      };
    },
  },
  mounted() {
    const seen = localStorage.getItem(`newFeatureSeen_${this.featureKey}`);
    if (!seen) {
      this.visible = true;
      this.startTime = performance.now();
      this.animateSpinner();
    }
  },
  methods: {
    animateSpinner() {
      const circumference = 2 * Math.PI * 20;
      const update = () => {
        if (!this.visible) return;
        const elapsed = performance.now() - this.startTime;
        if (elapsed >= this.duration) {
          this.close();
          return;
        }
        this.strokeDashoffset = circumference * (1 - elapsed / this.duration);
        requestAnimationFrame(update);
      };
      requestAnimationFrame(update);
    },
    close() {
      this.visible = false;
      localStorage.setItem(`newFeatureSeen_${this.featureKey}`, "true");
      this.$emit("closed");
    },
  },
};
</script>

<style scoped>
.overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.7); /* Fondo más oscuro */
  z-index: 9999;
  display: flex;
  justify-content: center;
  align-items: center;
}

.tooltip {
  position: absolute;
  background: radial-gradient(circle, #1e1e2f, #2c2c3e); /* Fondo moderno con degradado oscuro */
  padding: 30px 40px;
  border-radius: 20px;
  max-width: 360px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.6), 0 0 15px rgba(255, 255, 255, 0.2); /* Efecto de brillo */
  text-align: center;
  z-index: 10000;
  color: #e0e0e0; /* Texto claro para contraste */
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
}

.tooltip h3 {
  margin-bottom: 15px;
  font-weight: 700;
  font-size: 1.6rem;
  color: #ffffff; /* Color destacado */
}

.tooltip p {
  margin-bottom: 25px;
  font-size: 1.1rem;
  line-height: 1.5;
  color: #d1d1d1; /* Color de texto secundario */
}

.spinner {
  margin-bottom: 20px;
  width: 60px;
  height: 60px;
  margin-left: auto;
  margin-right: auto;
}

svg {
  transform: rotate(-90deg);
  width: 60px;
  height: 60px;
}

.path {
  stroke: #4caf50; /* Color del spinner */
  stroke-linecap: round;
  stroke-dasharray: 125.6;
  transition: stroke-dashoffset 0.1s linear;
}

/* Botón para cerrar el tooltip */
.btn-confirm {
  background-color: #4caf50;
  border: none;
  border-radius: 8px;
  color: white;
  font-weight: 700;
  padding: 0.8rem 1.6rem;
  cursor: pointer;
  transition: background-color 0.3s ease, box-shadow 0.3s ease;
  font-size: 1.1rem;
  box-shadow: 0 6px 12px rgba(76, 175, 80, 0.5), 0 0 10px rgba(255, 255, 255, 0.2); /* Efecto de brillo en el botón */
}

.btn-confirm:hover {
  background-color: #388e3c;
  box-shadow: 0 8px 16px rgba(56, 142, 60, 0.6), 0 0 15px rgba(255, 255, 255, 0.3); /* Más brillo al pasar el cursor */
}
</style>
