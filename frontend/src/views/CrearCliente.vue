<template>
  <ClienteForm @submit="crear" />
</template>

<script>
import ClienteForm from "@/components/ClienteForm.vue";
import UsuarioService from "@/services/UsuarioService";

export default {
  components: { ClienteForm },
  methods: {
    crear(clienteData) {
      UsuarioService.crearCliente(clienteData).then(() => {
        this.$router.push({ name: "Clientes" });
      });
    },
  },
};
</script>
