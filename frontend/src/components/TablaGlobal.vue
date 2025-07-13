<template>
  <DataTable v-bind="$attrs" v-on="$listeners">
    <!-- Columnas y filtros -->
    <slot />

    <!-- Estado vacío animado -->
    <template #empty>
      <EmptyState />
    </template>
  </DataTable>
</template>

<script>
import DataTable from "primevue/datatable";
import EmptyState from "./EmptyState.vue";

export default {
  name: "TablaGlobal",
  components: {
    DataTable,
    EmptyState,
  },
};
</script>
