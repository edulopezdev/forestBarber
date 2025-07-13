<template>
  <div class="empty-state">
    <div ref="lottieContainer" class="empty-lottie"></div>
    <p class="empty-text">¡Buuuu! Pasó el fantasma y no dejó ni un dato.</p>
    <small class="hint">Capaz que los filtros están más exigentes que vos en una cita</small>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import lottie from 'lottie-web'
import animationData from '@/animations/fantoma.json'  // <-- Importa el JSON

const lottieContainer = ref(null)
let animationInstance = null

onMounted(() => {
  animationInstance = lottie.loadAnimation({
    container: lottieContainer.value,
    renderer: 'svg',
    loop: true,
    autoplay: true,
    animationData,  // <-- usa el JSON importado
  })
})

onBeforeUnmount(() => {
  if (animationInstance) {
    animationInstance.destroy()
  }
})
</script>

<style scoped>
.empty-state {
  text-align: center;
  padding: 2rem;
  opacity: 0.9;
  animation: fadeIn 0.6s ease-in-out;
}

.empty-lottie {
  width: 120px;
  height: 120px;
  margin: 0 auto 1rem auto;
}

.empty-text {
  font-size: 1.1rem;
  color: #9ca3af;
  margin: 0;
}

.hint {
  color: #aaa;
  font-style: italic;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
