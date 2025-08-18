<template>
  <div v-if="visible" class="loading-spinner-overlay">
    <div class="spinner"></div>
    <p v-if="text" class="loading-text">{{ text }}</p>
  </div>
</template>

<script setup>
const props = defineProps({
  visible: { type: Boolean, default: false },
  text: { type: String, default: '' }
})
</script>

< scoped>
/* Spinner visual simple */
.loading-spinner-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  z-index: 9999;
  background: rgba(24,24,27,0.85);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.spinner {
  margin: 40px auto 20px auto;
  border: 8px solid #f3f3f3;
  border-top: 8px solid #333;
  border-radius: 50%;
  width: 80px;
  height: 80px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
.loading-text {
  margin-top: 1.5rem;
  font-size: 1.2rem;
  color: #28a745;
  font-weight: bold;
  letter-spacing: 1px;
}
