<template>
  <div class="clientes-container">
    <Toast />
    <Card>
      <template #title>
        <div class="encabezado-acciones">
          <h4>Cierre Diario</h4>
          <div class="botones-acciones">
            <div class="fecha-selector-container">
              <div class="fecha-wrapper">
                <i class="pi pi-calendar fecha-icon"></i>
                <Calendar
                  v-model="fechaSeleccionada"
                  dateFormat="dd/mm/yy"
                  :showIcon="false"
                  :maxDate="new Date()"
                  @date-select="obtenerCierrePorFecha"
                  v-tooltip.bottom="'Seleccionar otra fecha para ver el cierre'"
                  placeholder="Seleccionar fecha"
                />
              </div>
            </div>
            <Button
              v-if="cierreActual?.cerrado"
              label="Exportar PDF"
              icon="pi pi-file-pdf"
              class="boton-nuevo-cliente"
              @click="exportarPDF"
              severity="secondary"
              v-tooltip.bottom="'Descargar reporte en PDF'"
            />
            <Button
              v-if="!cierreActual?.cerrado"
              label="Cerrar Caja"
              icon="pi pi-lock"
              class="boton-nuevo-cliente"
              @click="abrirModalCierre"
              severity="success"
            />
          </div>
        </div>
      </template>

      <template #content>
        <div class="resumen-container">
          <!-- Resumen unificado -->
          <TablaGlobal
            :value="resumenUnificado"
            :paginator="false"
            class="tabla-resumen"
          >
            <Column
              field="concepto"
              header="Concepto"
              headerClass="header-align-left header-column"
            >
              <template #body="{ data }">
                <div
                  :class="{
                    'seccion-header': data.esHeader,
                    'resaltado-total': data.total,
                    'fila-concepto': !data.esHeader && !data.total,
                  }"
                  :colspan="data.esHeader ? 2 : null"
                >
                  {{ data.concepto }}
                </div>
              </template>
            </Column>
            <Column
              field="monto"
              header="Monto"
              headerClass="header-align-right header-column"
              style="width: 40%"
            >
              <template #body="{ data }">
                <span
                  :class="{
                    'monto-total': data.total,
                    oculto: data.esHeader,
                    'fila-monto': !data.esHeader && !data.total,
                  }"
                >
                  $ {{ data.monto !== undefined ? data.monto.toFixed(2) : "" }}
                </span>
              </template>
            </Column>
          </TablaGlobal>

          <!-- Panel de estado -->
          <div class="estado-panel">
            <!-- Diferencia -->
            <div
              class="estado-item diferencia-info"
              v-if="diferenciaPagos !== 0"
            >
              <div class="estado-icon warning-icon">⚠️</div>
              <div class="estado-content">
                <div class="estado-label">Diferencia detectada</div>
                <div class="estado-value resaltado-diferencia">
                  $ {{ diferenciaPagos.toFixed(2) }}
                </div>
              </div>
            </div>

            <!-- Estado -->
            <div class="estado-item" style="margin-top: 1.5rem">
              <div
                class="estado-icon"
                :class="cierreActual?.cerrado ? 'closed-icon' : 'open-icon'"
              >
                <i
                  :class="
                    cierreActual?.cerrado ? 'pi pi-lock' : 'pi pi-lock-open'
                  "
                ></i>
              </div>
              <div class="estado-content">
                <div class="estado-label">Estado de caja</div>
                <div
                  class="estado-value"
                  :class="
                    cierreActual?.cerrado ? 'estado-cerrado' : 'estado-abierto'
                  "
                >
                  {{ cierreActual?.cerrado ? "Cerrado" : "Abierto" }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <!-- Modal de cierre -->
    <Dialog
      v-model:visible="mostrarModalCierre"
      header="Confirmar cierre de caja"
      :modal="true"
      :closable="false"
      style="width: 450px"
      class="modal-cierre-caja"
    >
      <div class="formulario-cierre">
        <h3>Cierre de Caja</h3>

        <div class="campo">
          <label for="observacion">Observaciones</label>
          <Textarea
            id="observacion"
            v-model="observacion"
            placeholder="Observaciones (opcional)"
            rows="3"
            autoResize
          />
        </div>

        <div class="campo">
          <label for="contrasena"
            >Contraseña <span class="obligatorio">*</span></label
          >
          <Password
            id="contrasena"
            v-model="contrasena"
            placeholder="Ingrese su contraseña"
            toggleMask
            :feedback="false"
          />
        </div>

        <div class="acciones-formulario">
          <div class="contenedor-boton">
            <Button
              class="btn-guardar"
              label="Confirmar"
              icon="pi pi-check"
              @click="cerrarCaja"
            />
          </div>

          <button class="btn-cerrar" @click="cerrarModal" aria-label="Cerrar">
            <i class="pi pi-times"></i>
          </button>
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script>
import TablaGlobal from "../components/TablaGlobal.vue";
import Column from "primevue/column";
import Button from "primevue/button";
import Calendar from "primevue/calendar";
import Card from "primevue/card";
import Dialog from "primevue/dialog";
import Textarea from "primevue/textarea";
import Password from "primevue/password";
import Toast from "primevue/toast";
import Tooltip from "primevue/tooltip";
import Swal from "sweetalert2";
import CajaService from "../services/CajaService.js";
import { jsPDF } from "jspdf";
import html2canvas from "html2canvas";

export default {
  directives: {
    tooltip: Tooltip,
  },
  components: {
    TablaGlobal,
    Column,
    Button,
    Calendar,
    Card,
    Dialog,
    Textarea,
    Password,
    Toast,
  },
  data() {
    return {
      fechaSeleccionada: new Date(),
      cierreActual: null,
      resumenVentas: [],
      resumenPagos: [],
      resumenUnificado: [],
      diferenciaPagos: 0,
      mostrarModalCierre: false,
      observacion: "",
      contrasena: "",
      nombreOperador: "",
    };
  },
  mounted() {
    this.obtenerCierrePorFecha(this.fechaSeleccionada);
  },
  methods: {
    async obtenerCierrePorFecha(fecha) {
      try {
        const response = await CajaService.getCierrePorFecha(
          fecha.toISOString().split("T")[0]
        );
        const cierre = response.data.cierre;

        this.cierreActual = cierre;

        // Obtener los montos de productos y servicios
        const montoProductos =
          cierre.totalProductosVendidos || cierre.totalMontoProductos || 0;
        const montoServicios =
          cierre.totalServiciosVendidos || cierre.totalMontoServicios || 0;

        const resumenVentas = [
          { concepto: "Productos", monto: montoProductos },
          { concepto: "Servicios", monto: montoServicios },
          {
            concepto: "TOTAL FINAL DEL DÍA",
            monto: montoProductos + montoServicios,
            total: true,
          },
        ];

        // Si hay un usuarioId en el cierre, obtener el nombre del operador
        if (cierre.usuarioId) {
          try {
            const usuarioResponse = await CajaService.getUsuarioPorId(
              cierre.usuarioId
            );
            if (usuarioResponse.data && usuarioResponse.data.usuario) {
              this.nombreOperador =
                usuarioResponse.data.usuario.nombre || "Desconocido";
            }
          } catch (err) {
            console.error("Error al obtener nombre de operador:", err);
            this.nombreOperador = "Desconocido";
          }
        }

        const resumenPagos = cierre.pagos.map((p) => ({
          concepto: `Pago con ${p.metodoPago}`,
          monto: p.monto,
        }));

        const totalPagos = cierre.pagos.reduce((acc, p) => acc + p.monto, 0);
        const totalVentas = montoProductos + montoServicios;

        this.resumenVentas = resumenVentas;
        this.resumenPagos = resumenPagos;
        this.diferenciaPagos = totalPagos - totalVentas;

        // Crear resumen unificado
        this.resumenUnificado = [
          { concepto: "Ventas del Día", esHeader: true },
          ...resumenVentas,
        ];

        // Solo agregar la sección de pagos si hay pagos registrados
        if (resumenPagos.length > 0) {
          this.resumenUnificado.push(
            { concepto: "Ingresos por Método de Pago", esHeader: true },
            ...resumenPagos
          );
        }
      } catch (error) {
        console.error("Error al obtener cierre:", error);
        this.cierreActual = null;
        this.resumenVentas = [];
        this.resumenPagos = [];
        this.resumenUnificado = [];
        this.diferenciaPagos = 0;
        this.nombreOperador = "";
      }
    },

    abrirModalCierre() {
      this.mostrarModalCierre = true;
    },

    cerrarModal() {
      this.mostrarModalCierre = false;
      this.observacion = "";
      this.contrasena = "";
    },

    mostrarToast({ icon = "info", title = "", timer = 3000 }) {
      const iconColors = {
        success: "#28a745",
        error: "#dc3545",
        warning: "#ffc107",
        info: "#17a2b8",
      };

      Swal.fire({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer,
        timerProgressBar: true,
        icon,
        title,
        background: "#18181b",
        color: "#ffffff",
        iconColor: iconColors[icon] || "#ffffff",
      });
    },

    async cerrarCaja() {
      if (!this.contrasena) {
        this.mostrarToast({
          icon: "warning",
          title: "Debes ingresar la contraseña",
        });
        return;
      }

      try {
        const payload = {
          fecha: this.fechaSeleccionada.toISOString(),
          observaciones: this.observacion,
          password: this.contrasena, // <-- aquí el nombre correcto
        };

        const response = await CajaService.cerrarCaja(payload);

        this.mostrarToast({
          icon: "success",
          title: response.data.message || "Caja cerrada correctamente",
        });

        this.mostrarModalCierre = false;
        this.observacion = "";
        this.contrasena = "";

        this.obtenerCierrePorFecha(this.fechaSeleccionada);
      } catch (error) {
        const mensaje =
          error.response?.data?.message ||
          "Error al cerrar caja. Intente nuevamente";

        this.mostrarToast({
          icon: "error",
          title: mensaje,
        });
      }
    },
    exportarPDF() {
      try {
        // Crear PDF con formato simple y legible
        const pdf = new jsPDF({
          orientation: "portrait",
          unit: "mm",
          format: "a5",
        });

        // Configurar fuentes tipo ticket
        pdf.setFont("courier", "normal");

        // Fecha formateada
        const fecha = this.fechaSeleccionada.toLocaleDateString("es-ES", {
          day: "2-digit",
          month: "2-digit",
          year: "numeric",
        });

        // Márgenes y posiciones
        const margenIzq = 15;
        const anchoPagina = 118; // A5 tiene 148mm de ancho, dejamos márgenes
        let posY = 20; // Margen superior adecuado

        // Encabezado
        pdf.setFontSize(14);
        pdf.setTextColor(0, 0, 0);
        pdf.setFont("courier", "bold");
        pdf.text("FOREST BARBER", 74, posY, { align: "center" });

        posY += 8;
        pdf.setFontSize(12);
        pdf.setFont("courier", "normal");
        pdf.text("CIERRE DE CAJA", 74, posY, { align: "center" });

        posY += 8;
        pdf.setFontSize(10);
        pdf.text(`Fecha: ${fecha}`, 74, posY, { align: "center" });

        // Línea separadora
        posY += 5;
        pdf.setDrawColor(0, 0, 0);
        pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);

        // Sección: Resumen de Ventas
        posY += 8;
        pdf.setFontSize(12);
        pdf.setTextColor(0, 0, 0);
        pdf.setFont("courier", "bold");
        pdf.text("RESUMEN DE VENTAS", 74, posY, { align: "center" });
        pdf.setFont("courier", "normal");

        // Tabla de ventas
        posY += 8;

        // Encabezados de tabla
        pdf.setFont("courier", "bold");
        pdf.setFontSize(10);
        pdf.text("CONCEPTO", margenIzq, posY);
        pdf.text("MONTO", margenIzq + anchoPagina - 5, posY, {
          align: "right",
        });
        pdf.setFont("courier", "normal");

        // Línea bajo encabezados
        posY += 2;
        pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);

        // Filas de ventas
        posY += 6;
        this.resumenVentas.forEach((item) => {
          // Aplicar estilo para totales
          if (item.total) {
            pdf.setFont("courier", "bold");
          } else {
            pdf.setFont("courier", "normal");
          }

          pdf.text(item.concepto, margenIzq, posY);
          pdf.text(`$${item.monto.toFixed(2)}`, margenIzq + anchoPagina, posY, {
            align: "right",
          });

          posY += 6;
        });

        // Línea separadora
        pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);
        posY += 6;

        // Sección: Métodos de Pago (si existen)
        if (this.resumenPagos.length > 0) {
          pdf.setFont("courier", "bold");
          pdf.setFontSize(12);
          pdf.text("METODOS DE PAGO", 74, posY, { align: "center" });
          pdf.setFont("courier", "normal");

          // Tabla de pagos
          posY += 8;

          // Encabezados de tabla
          pdf.setFont("courier", "bold");
          pdf.setFontSize(10);
          pdf.text("METODO", margenIzq, posY);
          pdf.text("MONTO", margenIzq + anchoPagina - 5, posY, {
            align: "right",
          });
          pdf.setFont("courier", "normal");

          // Línea bajo encabezados
          posY += 2;
          pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);

          // Filas de pagos
          posY += 6;
          this.resumenPagos.forEach((item) => {
            pdf.text(item.concepto, margenIzq, posY);
            pdf.text(
              `$${item.monto.toFixed(2)}`,
              margenIzq + anchoPagina,
              posY,
              { align: "right" }
            );
            posY += 6;
          });

          // Línea separadora
          pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);
          posY += 6;
        }

        // Diferencia (si existe)
        if (this.diferenciaPagos !== 0) {
          pdf.setFont("courier", "bold");
          pdf.setFontSize(10);
          pdf.text("DIFERENCIA:", margenIzq, posY);
          pdf.text(
            `$${this.diferenciaPagos.toFixed(2)}`,
            margenIzq + anchoPagina,
            posY,
            { align: "right" }
          );
          posY += 6;

          // Línea separadora
          pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);
          posY += 6;
        }

        // Estado de caja
        pdf.setFont("courier", "bold");
        pdf.setFontSize(10);
        pdf.text("ESTADO:", margenIzq, posY);
        pdf.setFont("courier", "normal");
        pdf.text(
          this.cierreActual?.cerrado ? "CERRADO" : "ABIERTO",
          margenIzq + anchoPagina,
          posY,
          { align: "right" }
        );
        posY += 6;

        // Información del operador
        pdf.setFont("courier", "bold");
        pdf.text("OPERADOR:", margenIzq, posY);
        pdf.setFont("courier", "normal");
        pdf.text(
          this.nombreOperador || "No especificado",
          margenIzq + anchoPagina,
          posY,
          { align: "right" }
        );
        posY += 6;

        // Observaciones (si existen)
        if (this.cierreActual?.observaciones) {
          pdf.setFont("courier", "bold");
          pdf.text("OBSERVACIONES:", margenIzq, posY);
          posY += 5;
          pdf.setFont("courier", "normal");
          const obs = pdf.splitTextToSize(
            this.cierreActual.observaciones,
            anchoPagina
          );
          pdf.text(obs, margenIzq, posY);
          posY += obs.length * 5;
        }

        // Línea separadora final
        pdf.line(margenIzq, posY, margenIzq + anchoPagina, posY);
        posY += 6;

        // Pie de página
        pdf.setFontSize(8);
        pdf.setFont("courier", "italic");
        pdf.text(`Generado: ${new Date().toLocaleString("es-ES")}`, 74, posY, {
          align: "center",
        });
        posY += 5;
        pdf.text("Forest Barber - Sistema de Gestion", 74, posY, {
          align: "center",
        });

        // Ajustar el tamaño del PDF al contenido
        const finalHeight = posY + 10; // Añadir un margen adecuado al final
        if (finalHeight < 210) {
          // A5 tiene 210mm de alto
          pdf.internal.pageSize.height = 210;
        } else {
          pdf.internal.pageSize.height = finalHeight;
        }

        // Guardar PDF con escala adecuada para visualización
        pdf.save(`Cierre_Caja_${fecha.replace(/\//g, "-")}.pdf`);

        this.mostrarToast({
          icon: "success",
          title: "El informe se ha generado correctamente",
        });
      } catch (error) {
        console.error("Error al generar PDF:", error);
        this.mostrarToast({
          icon: "error",
          title: "No se pudo generar el PDF. Intente nuevamente.",
        });
      }
    },
  },
};
</script>

<style scoped>
.clientes-container {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  color: #e0e0e0;
}

.encabezado-acciones {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: -2.5rem;
  margin-top: -1rem;
}

.botones-acciones {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.fecha-selector-container {
  display: flex;
  align-items: center;
}

.fecha-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  background-color: #1c1c1c;
  border: 1px solid #444;
  border-radius: 6px;
  padding: 0 0.5rem;
  transition: all 0.2s ease;
}

.fecha-wrapper:hover {
  border-color: #666;
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.1);
}

.fecha-icon {
  color: #999;
  margin-right: 0.5rem;
  font-size: 0.9rem;
}

:deep(.p-calendar) {
  background-color: transparent;
  border: none;
}

:deep(.p-calendar .p-inputtext) {
  background-color: transparent;
  color: #ffffff;
  border: none;
  padding: 0.4rem 0.25rem;
  font-size: 0.85rem;
  width: 140px;
}

:deep(.p-calendar .p-inputtext:focus) {
  box-shadow: none;
  outline: none;
}

/* Estilos para el panel del calendario */
:deep(.p-datepicker) {
  background-color: #1e1e1e !important;
  border: 1px solid #444 !important;
  border-radius: 6px !important;
}

:deep(.p-datepicker-header) {
  background-color: #2a2a2a !important;
  border-bottom: 1px solid #444 !important;
}

/* Mejorar visibilidad de las flechas de navegación del calendario */
:deep(.p-datepicker .p-datepicker-header .p-datepicker-prev),
:deep(.p-datepicker .p-datepicker-header .p-datepicker-next) {
  color: white !important;
  border: 1px solid #666 !important;
  background-color: #333 !important;
  border-radius: 50%;
  width: 2rem;
  height: 2rem;
  transition: background-color 0.2s;
}

:deep(.p-datepicker .p-datepicker-header .p-datepicker-prev:hover),
:deep(.p-datepicker .p-datepicker-header .p-datepicker-next:hover) {
  background-color: #444 !important;
  border-color: #888 !important;
}

/* Hacer visibles los iconos de flechas */
:deep(.p-datepicker .p-datepicker-header .p-datepicker-prev span),
:deep(.p-datepicker .p-datepicker-header .p-datepicker-next span) {
  color: white !important;
}

/* Asegurar que los iconos de flechas sean visibles */
:deep(.p-datepicker .p-datepicker-header .pi-chevron-left),
:deep(.p-datepicker .p-datepicker-header .pi-chevron-right) {
  color: white !important;
  font-size: 1rem !important;
}

:deep(.p-datepicker .p-datepicker-header .p-datepicker-title) {
  color: white !important;
}

:deep(.p-datepicker-calendar th) {
  color: #999 !important;
}

:deep(.p-datepicker-calendar td) {
  border: 1px solid #333 !important;
}

:deep(.p-datepicker-calendar td > span) {
  color: #ddd !important;
}

:deep(.p-datepicker-calendar td > span:hover) {
  background-color: #444 !important;
}

:deep(.p-datepicker-calendar td > span.p-highlight) {
  background-color: #28a745 !important;
  color: white !important;
}

.boton-nuevo-cliente {
  background-color: #28a745;
  color: white;
  font-weight: normal;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.7rem;
  min-width: 120px;
}
.boton-nuevo-cliente:hover {
  background-color: #218838;
}

:deep(.p-datatable) {
  background-color: #121212;
  color: #eee;
  border-radius: 10px;
  border: none;
  box-shadow: 0 3px 8px rgb(0 0 0 / 0.5);
  font-size: 0.95rem;
  margin-bottom: 1.5rem;
}

:deep(.p-datatable-tbody > tr > td),
:deep(.p-datatable-thead > tr > th) {
  padding: 0.4rem 0.75rem !important;
  line-height: 1.2rem;
  border-bottom: 1px solid #333 !important;
  vertical-align: middle;
}

:deep(.p-datatable-thead > tr > th) {
  background-color: #2a2a2a;
  color: #ffffff;
  font-weight: 600;
  border-bottom: 2px solid #444 !important;
}

:deep(.header-align-left) {
  text-align: left !important;
  padding-left: 1rem !important;
}

:deep(.header-align-right) {
  text-align: right !important;
  padding-right: 1rem !important;
}
/* Eliminar completamente el efecto hover */
:deep(.p-datatable-tbody > tr) {
  pointer-events: none !important;
}

.resaltado-total {
  font-weight: bold;
  color: #66ff66;
}

.seccion-header {
  font-weight: 600;
  font-size: 1rem;
  color: #ffffff;
  padding: 0.7rem 0.75rem;
  background-color: #9400a147;
  text-align: left !important;
  border-bottom: 1px solid #444;
  border-radius: 6px 6px 0 0;
  margin-top: 0.5rem;
  position: relative;
  width: 100%;
  box-sizing: border-box;
  grid-column: 1 / -1; /* Hace que el encabezado ocupe todas las columnas */
  margin-left: -0.75rem; /* Extiende a la izquierda */
  margin-right: -0.75rem; /* Extiende a la derecha */
  width: calc(100% + 1.5rem); /* Compensa los márgenes negativos */
}

.oculto {
  visibility: hidden;
}

.total-clientes {
  margin-top: 1.9rem;
  font-size: 1rem;
  font-weight: 500;
  text-align: left;
  color: #aeaeae;
}

.diferencia-info {
  margin-top: 1rem;
  font-size: 1rem;
  color: #ffc107;
}

.resaltado-diferencia {
  font-weight: bold;
  color: #ff5555;
}

/* Estilos para el modal de cierre de caja */
.formulario-cierre {
  max-width: 400px;
  margin: 0 auto;
  padding: 1rem;
}

.formulario-cierre h3 {
  margin-bottom: 1.5rem;
  color: #ffffff;
  text-align: center;
}

.campo {
  margin-bottom: 1.5rem;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
}

label {
  font-weight: 600;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: #e0e0e0;
}

.obligatorio {
  color: #e74c3c;
  font-size: 1.2rem;
  line-height: 1;
  font-weight: 900;
  user-select: none;
  animation: pulse 1.5s infinite alternate ease-in-out;
  margin-left: 0.1rem;
}

@keyframes pulse {
  0% {
    opacity: 1;
    transform: scale(1);
  }
  100% {
    opacity: 0.6;
    transform: scale(1.2);
  }
}

/* Botones del modal */
.acciones-formulario {
  display: flex;
  justify-content: space-between;
  margin-top: 2rem;
  align-items: center;
}

.btn-cerrar {
  background: none;
  border: none;
  color: #999;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem;
}

.btn-cerrar:hover {
  color: #4a90e2;
}

.contenedor-boton {
  flex: 1;
}

.btn-guardar {
  background-color: #28a745 !important;
  color: white !important;
  border: none !important;
  border-radius: 6px !important;
  padding: 0.5rem 1.2rem !important;
  transition: background-color 0.3s ease, box-shadow 0.3s ease !important;
}

.btn-guardar:hover {
  background-color: #218838 !important;
  box-shadow: 0 0 8px #21883888 !important;
}

:deep(.modal-cierre-caja .p-dialog-header) {
  background-color: #1a1a1a;
  color: white;
  border-bottom: 1px solid #444;
}

:deep(.modal-cierre-caja .p-dialog-content) {
  background-color: #1a1a1a;
  color: #e0e0e0;
  padding: 0;
}

:deep(.modal-cierre-caja .p-inputtext) {
  background-color: #121212;
  color: white;
  border: 1px solid #444;
}

:deep(.modal-cierre-caja .p-password-input) {
  background-color: #121212;
  color: white;
  border: 1px solid #444;
}

/* Estilos para alineación de columnas */
.fila-concepto {
  text-align: left !important;
  padding-left: 1rem !important;
  display: block;
  width: 100%;
}

.fila-monto {
  text-align: right !important;
  padding-right: 9.5rem !important;
  display: block;
  width: 100%;
}

:deep(.p-datatable-tbody > tr > td) {
  text-align: center;
}

.resaltado-total {
  text-align: left !important;
  padding-left: 1rem !important;
  font-weight: bold;
  color: #66ff66;
}

/* Estilo específico para el monto del total */
.monto-total {
  text-align: right !important;
  padding-right: 9.5rem !important;
  display: block;
  width: 100%;
  font-weight: bold;
  color: #66ff66;
}

/* Estilos para mejorar el centrado de las columnas */
:deep(.p-datatable .p-datatable-thead > tr > th:first-child) {
  text-align: left !important;
  width: 60%;
}

:deep(.p-datatable .p-datatable-thead > tr > th:last-child) {
  text-align: right !important;
  width: 40%;
}

:deep(.tabla-resumen) {
  width: 100%;
  margin: 0 auto;
}
</style>
