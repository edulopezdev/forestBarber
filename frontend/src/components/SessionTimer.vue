

<template>
  <div 
    class="session-timer" 
    :class="timerColorClass"
    v-tooltip="'Tiempo restante de sesión. La sesión se cerrará automáticamente por seguridad.'"
  >
    <svg 
      class="timer-icon" 
      :class="{ 'shake-animation': shouldShake, 'pulse-animation': shouldPulse }"
      xmlns="http://www.w3.org/2000/svg" 
      width="28" 
      height="28" 
      viewBox="0 0 24 24" 
      fill="none" 
      :stroke="iconColor" 
      stroke-width="2" 
      stroke-linecap="round" 
      stroke-linejoin="round"
    >
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </svg>
    <span class="timer-text">{{ formattedTime }}</span>
    <span class="session-label">Sesión</span>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import Swal from 'sweetalert2';
import { useRouter } from 'vue-router';
import authService from '../services/auth.service';

const SESSION_DURATION = 4 * 60 * 60; // 4 horas en segundos
const STORAGE_KEY = 'session_timer_expiry';
const timer = ref(SESSION_DURATION);
const router = useRouter();
let intervalId = null;
let hasShownWarning = false;
let hasShownFinalWarning = false;
const userActivity = ref(false);
const activityTimeout = ref(null);
const SESSION_IDLE_THRESHOLD = 5 * 60; // 5 minutos de inactividad para considerar que hay actividad real

const formattedTime = computed(() => {
  const hours = Math.floor(timer.value / 3600);
  const minutes = Math.floor((timer.value % 3600) / 60);
  const seconds = timer.value % 60;
  
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
});

// Estados visuales basados en tiempo restante
const timerColorClass = computed(() => {
  const remainingMinutes = Math.floor(timer.value / 60);
  if (remainingMinutes <= 5) return 'timer-critical';
  if (remainingMinutes <= 15) return 'timer-warning';
  if (remainingMinutes <= 30) return 'timer-caution';
  return 'timer-normal';
});

const iconColor = computed(() => {
  const remainingMinutes = Math.floor(timer.value / 60);
  if (remainingMinutes <= 5) return '#ef4444'; // rojo
  if (remainingMinutes <= 15) return '#f97316'; // naranja
  if (remainingMinutes <= 30) return '#eab308'; // amarillo
  return '#4a90e2'; // azul normal
});

const shouldShake = computed(() => {
  return Math.floor(timer.value / 60) <= 5; // vibrar cuando queden 5 minutos o menos
});

const shouldPulse = computed(() => {
  return Math.floor(timer.value / 60) <= 15 && Math.floor(timer.value / 60) > 5; // pulsar entre 6-15 minutos
});

function getExpiry() {
  const expiry = localStorage.getItem(STORAGE_KEY);
  return expiry ? parseInt(expiry, 10) : null;
}

function setExpiry(secondsFromNow) {
  // Guardamos la hora absoluta de expiración
  const expiry = Math.floor(Date.now() / 1000) + secondsFromNow;
  localStorage.setItem(STORAGE_KEY, expiry);
}

function getInitialExpiry() {
  // Si hay una expiración almacenada, la usamos
  // Si no hay, creamos una nueva 
  const expiry = getExpiry();
  if (expiry) {
    return expiry;
  } else {
    const newExpiry = Math.floor(Date.now() / 1000) + SESSION_DURATION;
    localStorage.setItem(STORAGE_KEY, newExpiry);
    return newExpiry;
  }
}

function clearExpiry() {
  localStorage.removeItem(STORAGE_KEY);
}

function updateTimerFromStorage() {
  const expiry = getExpiry();
  if (expiry) {
    const now = Math.floor(Date.now() / 1000);
    timer.value = Math.max(0, expiry - now);
  } else {
    // Solo establecemos un nuevo tiempo si no hay uno ya guardado
    const newExpiry = Math.floor(Date.now() / 1000) + SESSION_DURATION;
    localStorage.setItem(STORAGE_KEY, newExpiry);
    timer.value = SESSION_DURATION;
  }
}

// Esta función solo debería usarse cuando detectamos actividad real del usuario
function extendSession() {
  const newExpiry = Math.floor(Date.now() / 1000) + SESSION_DURATION;
  localStorage.setItem(STORAGE_KEY, newExpiry);
  timer.value = SESSION_DURATION;
}

function startTimer() {
  // Usamos el valor existente o creamos uno nuevo si no hay
  updateTimerFromStorage();
  
  // Solo iniciar el timer si hay una sesión autenticada
  if (authService.isAuthenticated()) {
    intervalId = setInterval(() => {
      // No actualizamos desde storage cada vez, solo contamos hacia abajo
      // para evitar desincronización por pequeñas diferencias de tiempo
      if (timer.value > 0) {
        timer.value--;
      }
    
    const remainingMinutes = Math.floor(timer.value / 60);
    
    // Notificación de advertencia a los 15 minutos
    if (remainingMinutes === 15 && !hasShownWarning) {
      hasShownWarning = true;
      Swal.fire({
        title: 'Aviso de sesión',
        text: 'Tu sesión expirará en 15 minutos. Guarda tu trabajo.',
        icon: 'warning',
        timer: 4000,
        showConfirmButton: false,
        background: '#18181b',
        color: '#fff',
        toast: true,
        position: 'top-end'
      });
    }
    
    // Notificación final a los 5 minutos
    if (remainingMinutes === 5 && !hasShownFinalWarning) {
      hasShownFinalWarning = true;
      Swal.fire({
        title: '¡Atención!',
        text: 'Tu sesión expirará en 5 minutos. Finaliza tus tareas.',
        icon: 'error',
        timer: 6000,
        showConfirmButton: false,
        background: '#18181b',
        color: '#fff',
        toast: true,
        position: 'top-end'
      });
    }
    
    if (timer.value <= 0) {
      clearInterval(intervalId);
      clearExpiry();
      Swal.fire({
        title: 'Sesión expirada',
        text: 'Por seguridad, tu sesión ha expirado. Debes volver a iniciar sesión.',
        icon: 'warning',
        confirmButtonText: 'OK',
        background: '#18181b',
        color: '#fff',
      }).then(() => {
        authService.logout();
        router.push('/login');
      });
    }
  }, 1000);
  }
}

function handleUserActivity() {
  // Solo extendemos la sesión cuando:
  // 1. Es la primera actividad detectada
  // 2. Ha pasado un tiempo significativo (5 min) desde la última actividad
  if (!userActivity.value) {
    userActivity.value = true;
    
    // Dar tiempo para que se registre actividad sostenida, no solo un refresh
    if (activityTimeout.value) {
      clearTimeout(activityTimeout.value);
    }
    
    activityTimeout.value = setTimeout(() => {
      // Después de actividad continua por algunos minutos, extender la sesión
      if (timer.value < SESSION_DURATION - 60) { // Solo si ha pasado al menos 1 minuto
        extendSession();
        hasShownWarning = false;
        hasShownFinalWarning = false;
      }
      userActivity.value = false;
    }, SESSION_IDLE_THRESHOLD * 1000);
  }
}

// Verifica si la sesión es nueva (login reciente)
function isNewSession() {
  // Verificamos si existe un timer válido en localStorage
  const expiry = getExpiry();
  if (!expiry) {
    // No hay timer guardado, es una sesión nueva
    return true;
  }
  
  const now = Math.floor(Date.now() / 1000);
  const timeRemaining = expiry - now;
  
  // Si el timer expiró o queda más tiempo del máximo posible, es sesión nueva
  if (timeRemaining <= 0 || timeRemaining > SESSION_DURATION) {
    return true;
  }
  
  return false;
}

onMounted(() => {
  // Solo establecemos un nuevo timer si es una sesión nueva o no hay autenticación
  if (isNewSession() || !authService.isAuthenticated()) {
    clearExpiry();
    setExpiry(SESSION_DURATION);
  }
  
  updateTimerFromStorage();
  startTimer();
  
  // Detectar actividad real del usuario (no solo refresh)
  window.addEventListener('click', handleUserActivity);
  window.addEventListener('keypress', handleUserActivity);
  window.addEventListener('mousemove', handleUserActivity);
  window.addEventListener('touchstart', handleUserActivity);
});

onUnmounted(() => {
  clearInterval(intervalId);
  if (activityTimeout.value) {
    clearTimeout(activityTimeout.value);
  }
  window.removeEventListener('click', handleUserActivity);
  window.removeEventListener('keypress', handleUserActivity);
  window.removeEventListener('mousemove', handleUserActivity);
  window.removeEventListener('touchstart', handleUserActivity);
});
</script>

<style scoped>
.session-timer {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(24, 24, 27, 0.85);
  border-radius: 1rem;
  padding: 0.2rem 0.8rem 0.2rem 0.4rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
  transition: all 0.3s ease;
}

/* Estados de color del timer */
.timer-normal {
  border: 1px solid rgba(74, 144, 226, 0.3);
}

.timer-caution {
  border: 1px solid rgba(234, 179, 8, 0.5);
  background: rgba(234, 179, 8, 0.1);
}

.timer-warning {
  border: 1px solid rgba(249, 115, 22, 0.5);
  background: rgba(249, 115, 22, 0.1);
}

.timer-critical {
  border: 1px solid rgba(239, 68, 68, 0.5);
  background: rgba(239, 68, 68, 0.1);
  animation: glow-red 2s ease-in-out infinite alternate;
}

.timer-icon {
  width: 28px;
  height: 28px;
  margin-right: 0.2rem;
  transition: all 0.3s ease;
}

/* Animación de vibración para momentos críticos */
.shake-animation {
  animation: shake 0.5s ease-in-out infinite;
}

/* Animación de pulso para advertencias */
.pulse-animation {
  animation: pulse 1.5s ease-in-out infinite;
}

.timer-text {
  font-size: 1.1rem;
  font-weight: 600;
  color: #fff;
  letter-spacing: 1px;
  transition: color 0.3s ease;
}

.session-label {
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.7);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* Keyframes para animaciones */
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-2px) rotate(-1deg); }
  75% { transform: translateX(2px) rotate(1deg); }
}

@keyframes pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

@keyframes glow-red {
  0% { box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3); }
  100% { box-shadow: 0 2px 16px rgba(239, 68, 68, 0.6); }
}

/* Estados críticos - cambiar color del texto */
.timer-critical .timer-text {
  color: #fecaca;
}

.timer-warning .timer-text {
  color: #fed7aa;
}

.timer-caution .timer-text {
  color: #fef3c7;
}
</style>
