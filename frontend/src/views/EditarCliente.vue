<template>
  <div>
    <ClienteForm :cliente="cliente" @guardar="editar" v-if="cliente" />
    <div v-else>Cargando cliente...</div>
  </div>
</template>

<script>
import ClienteForm from "@/components/ClienteForm.vue";
import UsuarioService from "@/services/UsuarioService";

export default {
  components: { ClienteForm },
  data() {
    return {
      cliente: null,
    };
  },
  mounted() {
    const id = this.$route.params.id;
    if (!id) {
      this.$router.push({ name: "Clientes" });
    } else {
      this.cargarCliente(id);
    }
  },
  watch: {
    // Si cambia el id en la ruta, recarga el cliente
    '$route.params.id'(nuevoId) {
      if (nuevoId) {
        this.cargarCliente(nuevoId);
      } else {
        this.$router.push({ name: "Clientes" });
      }
    }
  },
  methods: {
    cargarCliente(id) {
      this.cliente = null; // para mostrar "Cargando..."
      UsuarioService.getCliente(id)
        .then(res => {
          this.cliente = res.data;
        })
        .catch(() => {
          this.$router.push({ name: "Clientes" });
        });
    },
    editar(clienteData) {
      UsuarioService.actualizarCliente(this.$route.params.id, clienteData)
        .then(() => {
          this.$router.push({ name: "Clientes" });
        })
        .catch((err) => {
          console.error("Error al actualizar:", err);
          alert("Error al actualizar el cliente.");
        });
    },
  },
};
</script>
