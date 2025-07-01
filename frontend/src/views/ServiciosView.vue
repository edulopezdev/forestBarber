<template>
  <div class="servicios-container">
    <Toast />
    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Servicios</h4>
          <div class="botones-acciones">
            <Button
              label="Filtros"
              icon="pi pi-filter"
              class="boton-filtros"
              @click="mostrarFiltros = !mostrarFiltros"
            />
            <Button
              label="Nuevo Servicio"
              icon="pi pi-plus"
              class="boton-nuevo-servicio"
              @click="abrirModalNuevo()"
            />
          </div>
        </div>
      </template>

      <template #content>
        <DataTable
          v-model:filters="filters"
          :value="servicios"
          :filterDisplay="mostrarFiltros ? 'row' : 'none'"
          lazy
          paginator
          :rows="pageSize"
          :totalRecords="totalServicios"
          tableStyle="min-width: 100%"
          :loading="loading"
          @page="onPageChange"
          @sort="onSort"
          @filter="onFilter"
          :customSort="true"
          sortMode="single"
          :autoLayout="true"
        >
          <!-- Nombre -->
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

          <!-- Descripción -->
          <Column field="descripcion" header="Descripción" sortable>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar por descripción"
              />
            </template>
          </Column>

          <!-- Precio -->
          <Column field="precio" header="Precio" sortable>
            <template #body="slotProps">
              {{ formatPrecio(slotProps.data.precio) }}
            </template>
            <template #filter="{ filterModel, filterCallback }">
              <InputText
                v-model="filterModel.value"
                @input="filterCallback()"
                placeholder="Buscar por precio"
              />
            </template>
          </Column>

          <!-- Imagen -->
          <Column
            field="imagen"
            header="Imagen"
            style="min-width: 100px"
            :filter="false"
            :showFilterMenu="false"
          >
            <template #body="slotProps">
              <img
                v-if="slotProps.data.rutaImagen"
                :src="getRutaImagen(slotProps.data.rutaImagen)"
                alt="Imagen Servicio"
                style="
                  width: 60px;
                  height: 60px;
                  object-fit: cover;
                  border-radius: 4px;
                "
              />
              <img
                v-else
                src="/img/no-image.jpg"
                alt="Sin imagen"
                style="
                  width: 60px;
                  height: 60px;
                  object-fit: cover;
                  border-radius: 4px;
                "
              />
            </template>
            <template #filter> </template>
          </Column>

          <!-- Acciones -->
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
                  v-tooltip.bottom="'Editar servicio'"
                  @click="abrirModalEditar(slotProps.data)"
                />
                <Button
                  v-if="esAdministrador"
                  icon="pi pi-trash"
                  severity="danger"
                  text
                  rounded
                  v-tooltip.bottom="'Eliminar servicio'"
                  @click="eliminarServicio(slotProps.data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>

        <div class="total-servicios" v-if="totalServicios > 0">
          Total de servicios registrados: {{ totalServicios }}
        </div>
      </template>
    </Card>

    <!-- Modal Nuevo/Editar -->
    <Dialog
      v-model:visible="mostrarModal"
      :header="servicioSeleccionado?.id ? 'Editar Servicio' : 'Nuevo Servicio'"
      :modal="true"
      :closeOnEscape="false"
      :closable="false"
      style="width: 450px"
    >
      <ProductoServicioForm
        :productoServicio="servicioSeleccionado"
        almacenablePorDefecto="false"
        :esServicio="true"
        @guardar="guardarServicio"
        @cancelar="cerrarModal"
      />
    </Dialog>

    <!-- Detalle (opcional por ahora) -->
    <Dialog
      v-model:visible="mostrarDetalleModal"
      header="Servicios"
      :modal="true"
      :closable="false"
      style="width: 450px"
    >
      <ProductoServicioDetalle
        :producto="servicioSeleccionado"
        :esServicio="true"
        @cerrar="mostrarDetalleModal = false"
      />
    </Dialog>
  </div>
</template>

<script>
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Card from "primevue/card";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Dialog from "primevue/dialog";
import Swal from "sweetalert2";
import { FilterMatchMode } from "primevue/api";
import authService from "../services/auth.service";

import ProductoServicioForm from "../components/ProductoServicioForm.vue";
import ProductoServicioDetalle from "../components/ProductoServicioDetalle.vue";

import productoServicioService from "../services/ProductoServicioService";

export default {
  name: "Servicios",
  components: {
    DataTable,
    Column,
    Card,
    Button,
    InputText,
    Dialog,
    ProductoServicioForm,
    ProductoServicioDetalle,
  },
  computed: {
    esAdministrador() {
      return authService.getUserRole() === "Administrador";
    },
  },
  data() {
    return {
      servicios: [],
      totalServicios: 0,
      currentPage: 1,
      pageSize: 6,
      first: 0,
      sortField: null,
      sortOrder: null,
      loading: false,
      mostrarFiltros: false,

      filters: {
        nombre: { value: null, matchMode: FilterMatchMode.CONTAINS },
        descripcion: { value: null, matchMode: FilterMatchMode.CONTAINS },
        precio: { value: null, matchMode: FilterMatchMode.EQUALS },
        imagen: { value: null, matchMode: FilterMatchMode.EQUALS },
      },

      mostrarModal: false,
      servicioSeleccionado: null,
      mostrarDetalleModal: false,
    };
  },
  mounted() {
    this.obtenerServicios(1, 6);
  },
  methods: {
    formatPrecio(precio) {
      return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      }).format(precio);
    },
    getRutaImagen(ruta) {
      if (!ruta) return "/img/no-image.png";
      const baseUrl =
        import.meta.env.VITE_API_BASE_URL || "http://localhost:5042";
      const fullUrl = ruta.startsWith("http")
        ? ruta
        : `${baseUrl}${ruta.startsWith("/") ? "" : "/"}${ruta}`;
      return fullUrl;
    },

    abrirModalNuevo() {
      this.servicioSeleccionado = null;
      this.mostrarModal = true;
      document.body.classList.add("modal-open");
    },
    abrirModalEditar(servicio) {
      this.servicioSeleccionado = { ...servicio };
      this.mostrarModal = true;
      document.body.classList.add("modal-open");
    },
    cerrarModal() {
      this.mostrarModal = false;
      document.body.classList.remove("modal-open");
    },
    verDetalles(servicio) {
      this.servicioSeleccionado = { ...servicio };
      this.mostrarDetalleModal = true;
    },
    eliminarServicio(servicio) {
      Swal.fire({
        title: `¿Eliminar "${servicio.nombre}"?`,
        text: "Esta acción no se puede deshacer.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#e74c3c",
        cancelButtonColor: "#6c757d",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar",
        background: "#18181b",
        color: "#fff",
      }).then((result) => {
        if (result.isConfirmed) {
          productoServicioService
            .eliminarServicio(servicio.id)
            .then(() => {
              Swal.fire({
                title: "Eliminado",
                text: `${servicio.nombre} eliminado correctamente.`,
                icon: "success",
                timer: 2000,
                showConfirmButton: false,
                background: "#18181b",
                color: "#fff",
              });
              this.obtenerServicios();
            })
            .catch((err) => {
              console.error("Error al eliminar servicio:", err);

              // Extraer mensaje del backend si existe
              let errorMessage = "Ocurrió un error desconocido.";
              let errorTitle = "Error";

              if (err.response) {
                const { status } = err.response;

                if (status === 403) {
                  errorMessage =
                    err.response.data.message ||
                    "No tienes permiso para realizar esta acción.";
                  errorTitle = "Acceso denegado";
                } else if (status === 400 || status === 404) {
                  errorMessage =
                    err.response.data.message ||
                    "No se pudo procesar la solicitud.";
                  errorTitle = "Advertencia";
                } else {
                  errorMessage =
                    err.response.data.message ||
                    "No se pudo eliminar el servicio.";
                }
              } else if (err.request) {
                errorMessage = "No se recibió respuesta del servidor.";
                errorTitle = "Sin conexión";
              } else {
                errorMessage = err.message;
              }

              Swal.fire({
                title: errorTitle,
                text: errorMessage,
                icon: "error",
                background: "#18181b",
                color: "#fff",
              });
            });
        }
      });
    },
    guardarServicio(formData) {
      if (this.servicioSeleccionado?.id) {
        productoServicioService
          .actualizarServicio(this.servicioSeleccionado.id, formData)
          .then(() => {
            Swal.fire({
              title: "Actualizado",
              text: "Servicio actualizado correctamente.",
              icon: "success",
              timer: 2000,
              showConfirmButton: false,
              background: "#18181b",
              color: "#fff",
            });
            this.obtenerServicios();
            this.cerrarModal();
          })
          .catch((err) => {
            console.error(err);
            Swal.fire("Error", "No se pudo actualizar el servicio.", "error");
          });
      } else {
        productoServicioService
          .crearServicio(formData)
          .then(() => {
            Swal.fire({
              title: "Creado",
              text: "Servicio creado correctamente.",
              icon: "success",
              timer: 2000,
              showConfirmButton: false,
              background: "#18181b",
              color: "#fff",
            });
            this.obtenerServicios();
            this.cerrarModal();
          })
          .catch((err) => {
            console.error(err);
            Swal.fire("Error", "No se pudo crear el servicio.", "error");
          });
      }
    },
    obtenerServicios(page = 1, pageSize = 6) {
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

      if (this.sortField) {
        filtrosAplicados.sort = this.sortField;
        filtrosAplicados.order = this.sortOrder === 1 ? "asc" : "desc";
      }

      productoServicioService
        .getServicios(page, pageSize, filtrosAplicados)
        .then((response) => {
          const data = response.data;
          this.servicios = data.productos;
          this.totalServicios = data.pagination.totalProductos;
          this.currentPage = page;
          this.pageSize = pageSize;
          this.first = (page - 1) * pageSize;
        })
        .catch((error) => {
          console.error("Error al obtener servicios:", error);
        })
        .finally(() => {
          this.loading = false;
        });
    },
    onPageChange(event) {
      this.obtenerServicios(event.page + 1, event.rows);
    },
    onSort(event) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder;
      this.obtenerServicios(this.currentPage, this.pageSize);
    },
    onFilter(event) {
      this.obtenerServicios(1, this.pageSize);
    },
  },
};
</script>

<style scoped>
/* ===========================
   CONTENEDOR GENERAL
=========================== */
.servicios-container {
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

.boton-nuevo-servicio {
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
.boton-nuevo-cliente:hover {
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

:deep(.p-datatable-thead > tr > th:nth-child(5)) /* Acciones */ {
  text-align: center !important;
  vertical-align: middle !important;
}

:deep(.p-datatable-thead > tr > th:nth-child(5) .p-column-header-content) {
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
.total-servicios {
  margin-top: 1.9rem;
  font-size: 1rem;
  font-weight: 500;
  text-align: left;
  color: #aeaeae;
}
</style>
