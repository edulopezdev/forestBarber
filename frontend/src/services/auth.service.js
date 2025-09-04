const KEY_TOKEN = "token";
const KEY_USER = "user";
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default {
  login(email, password) {
    // Llamada a la API .NET
    const url = `${API_BASE_URL}/api/auth/login`;
    axios
      .post(url, { email, password })
      .then((res) => {
        sessionStorage.setItem(KEY_TOKEN, res.data.token);
        sessionStorage.setItem(KEY_USER, JSON.stringify(res.data.usuario));
        window.location.href = "/dashboard";
      })
      .catch((err) => {
        alert("Usuario o contraseña incorrectos");
      });
  },

  logout() {
    sessionStorage.removeItem(KEY_TOKEN);
    sessionStorage.removeItem(KEY_USER);
    // Limpiar también el timer de sesión
    localStorage.removeItem('session_timer_expiry');
    window.location.href = "/login";
  },

  getToken() {
    return sessionStorage.getItem(KEY_TOKEN);
  },

  getUser() {
    const userStr = sessionStorage.getItem(KEY_USER);
    return userStr ? JSON.parse(userStr) : null;
  },

  isAuthenticated() {
    return !!this.getToken();
  },

  isRole(role) {
    const user = this.getUser();
    return user && user.rol === role;
  },
  getUserRole() {
  const user = this.getUser();
  return user ? user.rolNombre : null; 
},

};
