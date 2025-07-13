<template>
  <div class="usuarios-container">
    <Toast />
    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Usuarios del Sistema</h4>
          <div class="botones-acciones">
            <Button
              label="Filtros"
              icon="pi pi-filter"
              class="boton-filtros"
              @click="mostrarFiltros = !mostrarFiltros"
            />
            <Button
              label="Nuevo Usuario"
              icon="pi pi-plus"
              class="boton-nuevo-usuario"
              @click="abrirModalNuevo()"
            />
          </div>
        </div>
      </template>

      <template #content>
        <TablaGlobal
          v-model:filters="filters"
          :value="usuarios"
          :filterDisplay="mostrarFiltros ? 'row' : 'none'"
          :globalFilterFields="['nombre', 'email', 'telefono', 'rolNombre']"
          lazy
          paginator
          :rows="pageSize"
          :first="first"
          :totalRecords="totalUsuarios"
          tableStyle="min-width: 100%"
          :loading="loading"
          @page="onPageChange"
          @sort="onSort"
          @filter="onFilter"
        >
          <Column field="nombre" sortable>
            <template #header>
              <span class="titulo-columna">Nombre</span>
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar por nombre"
              />
            </template>
          </Column>

          <Column field="email" header="Email" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar por email"
              />
            </template>
          </Column>

          <Column field="telefono" header="Teléfono" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar por teléfono"
              />
            </template>
          </Column>

          <Column field="rolNombre" header="Rol" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <Dropdown
                v-model="filterModel.value"
                @change="filterCallback()"
                :options="rolesDropdown"
                optionLabel="label"
                optionValue="value"
                placeholder="Seleccionar rol"
                showClear
              />
            </template>
            <template #body="slotProps">
              {{ slotProps.data.rolNombre }}
            </template>
          </Column>

          <Column field="activo" header="Estado">
            <template #body="slotProps">
              <Tag
                :value="slotProps.data.activo ? 'Activo' : 'Inactivo'"
                :severity="slotProps.data.activo ? 'success' : 'danger'"
              />
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <Dropdown
                v-model="filterModel.value"
                @change="filterCallback()"
                :options="[
                  { label: 'Activo', value: true },
                  { label: 'Inactivo', value: false },
                ]"
                optionLabel="label"
                placeholder="Seleccionar estado"
                showClear
              />
            </template>
          </Column>

          <Column header="Acciones" style="min-width: 180px">
            <template #body="slotProps">
              <div class="acciones-botones">
                <Button
                  icon="pi pi-eye"
                  severity="info"
                  text
                  rounded
                  v-tooltip.bottom="'Ver detalles'"
                  @click="verDetalles(slotProps.data)"
                />
                <Button
                  icon="pi pi-pencil"
                  severity="warning"
                  text
                  rounded
                  v-tooltip.bottom="'Editar usuario'"
                  @click="abrirModalEditar(slotProps.data)"
                />
                <Button
                  v-if="
                    usuarioLogueadoId !== null &&
                    (slotProps.data.id !== usuarioLogueadoId ||
                      !slotProps.data.activo)
                  "
                  :icon="
                    slotProps.data.activo ? 'pi pi-user-minus' : 'pi pi-user-plus'
                  "
                  :severity="slotProps.data.activo ? 'danger' : 'success'"
                  text
                  rounded
                  v-tooltip.bottom=" slotProps.data.activo
                      ? 'Desactivar usuario'
                      : 'Reactivar usuario'"
                  @click="
                    slotProps.data.activo
                      ? eliminarUsuario(slotProps.data)
                      : reactivarUsuario(slotProps.data)
                  "
                />
              </div>
            </template>
          </Column>
        </TablaGlobal>

        <div class="total-usuarios" v-if="totalUsuarios > 0">
          Total de usuarios registrados: {{ totalUsuarios }}
        </div>
      </template>
    </Card>

    <Dialog
      v-model:visible="mostrarModal"
      :header="usuarioSeleccionado?.id ? 'Editar Usuario' : 'Nuevo Usuario'"
      :modal="true"
      :closeOnEscape="false"
      :closeOnBackdropClick="false"
      :closable="false"
      style="width: 450px"
    >
      <UsuarioForm
        :usuario="usuarioSeleccionado"
        @guardar="guardarUsuario"
        @cancelar="cerrarModal"
      />
    </Dialog>

    <Dialog
      v-model:visible="mostrarDetalleModal"
      header="Detalle del Usuario"
      :modal="true"
      :closable="false"
      style="width: 450px"
    >
      <UsuarioDetalle
        :usuario="usuarioSeleccionado"
        @cerrar="mostrarDetalleModal = false"
      />
    </Dialog>
  </div>
</template>

<script>
import UsuarioService from "../services/UsuarioService";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Card from "primevue/card";
import Tag from "primevue/tag";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Dropdown from "primevue/dropdown";
import { FilterMatchMode } from "primevue/api";
import Dialog from "primevue/dialog";
import UsuarioForm from "../components/UsuarioForm.vue";
import UsuarioDetalle from "../components/UsuarioDetalle.vue";
import Swal from "sweetalert2";
import authService from "../services/auth.service";

export default {
  components: {
    DataTable,
    Column,
    Card,
    Tag,
    Button,
    InputText,
    Dropdown,
    Dialog,
    UsuarioForm,
    UsuarioDetalle,
  },
  data() {
    return {
      usuarioLogueadoId: authService.getUser()?.id || null,
      mostrarFiltros: false,
      usuarios: [],
      totalUsuarios: 0,
      currentPage: 1,
      pageSize: 10,
      first: 0,
      sortField: null,
      sortOrder: null,
      loading: false,
      filters: {
        nombre: { value: null, matchMode: FilterMatchMode.STARTS_WITH },
        email: { value: null, matchMode: FilterMatchMode.CONTAINS },
        telefono: { value: null, matchMode: FilterMatchMode.CONTAINS },
        rolNombre: { value: null, matchMode: FilterMatchMode.EQUALS },
        activo: { value: true, matchMode: FilterMatchMode.EQUALS },
      },

      rolesDropdown: [
        { label: "Administrador", value: "Administrador" },
        { label: "Barbero", value: "Barbero" },
      ],

      mostrarModal: false,
      usuarioSeleccionado: null,
      mostrarDetalleModal: false,
    };
  },
  mounted() {
    this.obtenerUsuarios();
  },
  methods: {
    abrirModalNuevo(usuario = null) {
      this.usuarioSeleccionado = usuario;
      this.mostrarModal = true;
      document.body.classList.add("modal-open");
    },
    abrirModalEditar(usuario) {
      this.usuarioSeleccionado = { ...usuario };
      this.mostrarModal = true;
      document.body.classList.add("modal-open");
    },
    cerrarModal() {
      this.mostrarModal = false;
      document.body.classList.remove("modal-open");
    },
    verDetalles(usuario) {
      UsuarioService.getUsuario(usuario.id)
        .then((res) => {
          this.usuarioSeleccionado = res.data.usuario;
          this.mostrarDetalleModal = true;
        })
        .catch(() => {
          Swal.fire(
            "Error",
            "No se pudo cargar el detalle del usuario.",
            "error"
          );
        });
    },
    eliminarUsuario(usuario) {
      Swal.fire({
        title: `¿Desactivar a ${usuario.nombre}?`,
        text: "Esta acción siempre se puede revertir",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#e74c3c",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, desactivar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then((result) => {
        if (result.isConfirmed) {
          UsuarioService.eliminarUsuario(usuario.id)
            .then(() => {
              Swal.fire({
                title: "Eliminado",
                text: `El usuario ${usuario.nombre} ha sido eliminado.`,
                icon: "success",
                timer: 2000,
                timerProgressBar: true,
                showConfirmButton: false,
                background: "#18181b",
                color: "#fff",
              });
              this.obtenerUsuarios(this.currentPage, this.pageSize);
            })
            .catch((error) => {
              // Extraer mensaje personalizado del backend o fallback
              const backendMessage =
                error?.response?.data?.message ||
                "No se pudo desactivar el usuario.";

              Swal.fire({
                title: "Error",
                text: backendMessage,
                icon: "error",
                background: "#18181b",
                color: "#fff",
              });
            });
        }
      });
    },

    reactivarUsuario(usuario) {
      Swal.fire({
        title: `¿Reactivar a ${usuario.nombre}?`,
        text: "El usuario volverá a estar activo.",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#28a745",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, reactivar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then((result) => {
        if (result.isConfirmed) {
          UsuarioService.cambiarEstado(usuario.id, true)
            .then(() => {
              Swal.fire({
                title: "Reactivado",
                text: `${usuario.nombre} ha sido reactivado.`,
                icon: "success",
                timer: 2000,
                showConfirmButton: false,
                background: "#18181b",
                color: "#fff",
              });
              this.obtenerUsuarios(this.currentPage, this.pageSize);
            })
            .catch(() => {
              Swal.fire("Error", "No se pudo reactivar el usuario.", "error");
            });
        }
      });
    },
    async guardarUsuario(usuarioActualizado) {
      if (usuarioActualizado.id) {
        this.cerrarModal();

        const result = await Swal.fire({
          title: `¿Actualizar a ${usuarioActualizado.nombre}?`,
          icon: "question",
          showCancelButton: true,
          confirmButtonColor: "#3085d6",
          cancelButtonColor: "#6c757d",
          confirmButtonText: "Sí, actualizar",
          cancelButtonText: "Cancelar",
          background: "#18181b",
          color: "#fff",
        });

        if (result.isConfirmed) {
          try {
            await UsuarioService.actualizarUsuario(
              usuarioActualizado.id,
              usuarioActualizado
            );
            await Swal.fire({
              title: "Actualizado",
              text: `Usuario ${usuarioActualizado.nombre} actualizado correctamente.`,
              icon: "success",
              timer: 2000,
              timerProgressBar: true,
              showConfirmButton: false,
              background: "#18181b",
              color: "#fff",
            });
            this.obtenerUsuarios();
            this.verDetalles(usuarioActualizado);
          } catch (error) {
            console.error("Error completo:", error);
            const mensaje =
              error?.response?.data?.message ||
              "No se pudo actualizar el usuario.";

            await Swal.fire({
              title: "Error",
              text: mensaje,
              icon: "error",
              background: "#18181b",
              color: "#fff",
            });

            this.abrirModalEditar({ ...usuarioActualizado });
          }
        } else {
          this.abrirModalEditar(usuarioActualizado);
        }
      } else {
        this.cerrarModal();

        const result = await Swal.fire({
          title: `¿Crear usuario ${usuarioActualizado.nombre}?`,
          icon: "question",
          showCancelButton: true,
          confirmButtonColor: "#3085d6",
          cancelButtonColor: "#6c757d",
          confirmButtonText: "Sí, crear",
          cancelButtonText: "Cancelar",
          background: "#18181b",
          color: "#fff",
        });

        if (result.isConfirmed) {
          try {
            await UsuarioService.crearUsuario(usuarioActualizado);
            await Swal.fire({
              title: "Creado",
              text: "Usuario creado correctamente.",
              icon: "success",
              timer: 2000,
              timerProgressBar: true,
              showConfirmButton: false,
              background: "#18181b",
              color: "#fff",
            });
            this.obtenerUsuarios();
          } catch (error) {
            console.error(error);

            const mensaje =
              error?.response?.data?.message || "No se pudo crear el usuario.";

            await Swal.fire({
              title: "Error",
              text: mensaje,
              icon: "error",
              background: "#18181b",
              color: "#fff",
            });

            this.abrirModalNuevo({ ...usuarioActualizado });
          }
        } else {
          this.abrirModalNuevo(usuarioActualizado);
        }
      }
    },
    async obtenerUsuarios(page = 1, pageSize = 10) {
      this.loading = true;

      const filtrosAplicados = {};
      Object.keys(this.filters).forEach((key) => {
        let val = this.filters[key]?.value;
        if (val !== null && val !== undefined && val !== "") {
          if (typeof val === "object" && val.hasOwnProperty("value")) {
            val = val.value;
          }
          filtrosAplicados[key] = val;
        }
      });

      if (this.sortField && this.sortOrder) {
        filtrosAplicados.ordenarPor = this.sortField;
        filtrosAplicados.ordenDescendente = this.sortOrder === -1;
      }

      try {
        const res = await UsuarioService.getUsuarios(
          page,
          pageSize,
          filtrosAplicados
        );

        this.usuarios = res.data.usuarios;
        this.totalUsuarios = res.data.pagination.total;
        this.pageSize = pageSize;
        this.currentPage = page;
        this.first = (page - 1) * pageSize;
      } catch (err) {
        console.error("Error al cargar usuarios:", err);
      } finally {
        this.loading = false;
      }
    },
    onPageChange(event) {
      this.obtenerUsuarios(event.page + 1, event.rows);
    },
    onSort(event) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder;
      this.obtenerUsuarios(this.currentPage, this.pageSize);
    },
    onFilter() {
      this.obtenerUsuarios(1, this.pageSize);
    },
  },
};
</script>

<style scoped>
/* ===========================
   CONTENEDOR GENERAL
=========================== */
.usuarios-container {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  color: #e0e0e0;
}

/* ===========================
   ENCABEZADO DE TABLA Y BOTONES
=========================== */
.encabezado-acciones {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: -2.5rem;
  margin-top: -1rem;
}

.boton-filtros {
  background-color: transparent !important;
  color: white !important;
  border: none !important;
  box-shadow: none !important;
  padding: 0.3rem 0.6rem !important;
  margin-right: 1rem !important;
}
.boton-filtros:focus,
.boton-filtros:focus-visible,
.boton-filtros:active {
  outline: none !important;
  box-shadow: none !important;
  border-color: transparent !important;
}
:deep(.boton-filtros:focus-visible) {
  outline: none !important;
  box-shadow: 0 0 0 2px rgba(40, 167, 69, 0.4) !important;
  border-color: transparent !important;
}

.boton-nuevo-usuario {
  background-color: #28a745;
  color: white;
  font-weight: normal;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.7rem;
  width: auto !important;
  height: auto !important;
  min-width: 120px !important;
}
.boton-nuevo-usuario:hover {
  background-color: #218838;
}

/* ===========================
   DATATABLE ESTILOS GENERALES
=========================== */
:deep(.p-datatable) {
  background-color: #121212;
  color: #eee;
  border-radius: 10px;
  border: none;
  box-shadow: 0 3px 8px rgb(0 0 0 / 0.5);
  font-family: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif;
  font-size: 0.95rem;
}

:deep(.p-datatable-tbody > tr > td),
:deep(.p-datatable-thead > tr > th) {
  padding: 0.4rem 0.75rem !important;
  line-height: 1.2rem;
  border-bottom: 1px solid #333 !important;
  vertical-align: middle;
  text-align: center !important;
}

:deep(.p-datatable-thead > tr > th) {
  background-color: #2a2a2a;
  color: #ffffff;
  font-weight: 600;
  text-align: center !important;
}

:deep(.p-datatable-tbody > tr:hover) {
  background-color: #1e3a1e;
}

:deep(.p-sortable-column .p-column-header-content) {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
}

:deep(.p-datatable-thead > tr > th:nth-child(1) .p-column-header-content) {
  gap: 0.1rem !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(4)) /* Estado */ {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(6)) /* Acciones */ {
  text-align: center !important;
  vertical-align: middle !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(6) .p-column-header-content) {
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  gap: 0.4rem;
}

/* ===========================
   ESTILOS DE FILTROS EN COLUMNAS
=========================== */
:deep(.p-column-filter .p-inputtext) {
  height: 28px !important;
  padding: 0.25rem 0.5rem !important;
  font-size: 0.85rem !important;
  margin: 0 !important;
  border-radius: 4px !important;
}

:deep(.p-column-filter .p-dropdown) {
  height: 28px !important;
  font-size: 0.85rem !important;
}

:deep(.p-column-filter .p-dropdown .p-dropdown-label) {
  line-height: 28px !important;
  padding: 0 0.5rem !important;
}

:deep(.p-column-filter) {
  padding: 0.15rem 0.3rem !important;
  margin: 0 !important;
  min-height: 40px;
}

:deep(.p-column-filter-menu-button) {
  background: transparent !important;
  border: none !important;
  padding: 0 !important;
  width: 14px;
  height: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

:deep(.p-column-filter-clear-button) {
  background: transparent !important;
  border: none !important;
  padding: 0 !important;
  width: auto !important;
  height: auto !important;
  min-width: 0 !important;
  min-height: 0 !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  box-shadow: none !important;
  outline: none !important;
}

:deep(.p-column-filter-clear-button > svg) {
  width: 16px !important;
  height: 16px !important;
  pointer-events: auto;
  fill: currentColor;
}

:deep(.p-column-filter-menu-button:focus),
:deep(.p-column-filter-menu-button:active) {
  outline: none !important;
  box-shadow: none !important;
}

/* ===========================
   PAGINADOR
=========================== */
:deep(.p-datatable .p-paginator) {
  background-color: transparent;
  border-top: 1px solid #444;
  padding-top: 0.4rem;
  font-size: 0.85rem;
  color: #ccc;
}

:deep(.p-datatable .p-paginator .p-highlight) {
  background-color: #28a745 !important;
  color: white !important;
  border-radius: 4px;
  font-weight: 600;
}

:deep(.p-paginator .p-paginator-page:focus),
:deep(.p-paginator .p-paginator-page:focus-visible),
:deep(.p-paginator .p-paginator-page.p-highlight) {
  outline: none !important;
  box-shadow: none !important;
  border-color: transparent !important;
}

/* ===========================
   BOTONES DE ACCIÓN EN FILA
=========================== */
.acciones-botones {
  display: flex;
  gap: 0.25rem;
  justify-content: center;
  align-items: center;
}

:deep(.p-button) {
  margin: 0 !important;
  padding: 0.25rem 0.35rem !important;
  font-size: 0.85rem !important;
  width: 30px;
  height: 30px;
  min-width: 30px !important;
  border-radius: 6px !important;
  line-height: 1;
}

:deep(.p-button:last-child) {
  margin-right: 0;
}

/* ===========================
   TAGS DE ESTADO (ACTIVO / INACTIVO)
=========================== */
:deep(.p-tag-success) {
  background-color: #27ae60 !important;
  color: #e0f2f1 !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}

:deep(.p-tag-danger) {
  background-color: #c0392b !important;
  color: #fdecea !important;
  font-weight: 600;
  border-radius: 12px !important;
  padding: 0.2rem 0.6rem !important;
}

/* ===========================
   TÍTULOS Y FILTROS AVANZADOS
=========================== */
.titulo-columna {
  font-weight: 600;
  color: white;
}

.columna-nombre-header {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 0.3rem;
}

.icono-filtro {
  position: absolute;
  left: -5rem;
  top: 50%;
  transform: translateY(-50%);
  color: white;
  font-size: 1rem;
  cursor: pointer;
  transition: transform 0.2s ease, color 0.2s ease;
  z-index: 10;
}
.icono-filtro:hover {
  transform: scale(1.2);
  color: #28a745;
}
.total-usuarios {
  margin-top: 1.9rem;
  font-size: 1rem;
  font-weight: 500;
  text-align: left;
  color: #aeaeae;
}
</style>
