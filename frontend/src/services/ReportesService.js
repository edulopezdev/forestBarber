import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

const ReportesService = {
    // Reporte completo del día
    async getReporteDiaCompleto(fecha) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/dia-completo`, {
                params: { fecha: fecha.toISOString().split('T')[0] }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener reporte del día:', error);
            throw error;
        }
    },

    // Solo turnos del día
    async getTurnosPorDia(fecha) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/turnos-dia`, {
                params: { fecha: fecha.toISOString().split('T')[0] }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener turnos del día:', error);
            throw error;
        }
    },

    // Solo atenciones del día
    async getAtencionesPorDia(fecha) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/atenciones-dia`, {
                params: { fecha: fecha.toISOString().split('T')[0] }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener atenciones del día:', error);
            throw error;
        }
    },

    // Reporte por barbero
    async getReporteBarbero(barberoId, fecha) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/barbero-dia`, {
                params: {
                    barberoId,
                    fecha: fecha.toISOString().split('T')[0]
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener reporte del barbero:', error);
            throw error;
        }
    },

    // Pagos del día
    async getPagosPorDia(fecha) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/pagos-dia`, {
                params: { fecha: fecha.toISOString().split('T')[0] }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener pagos del día:', error);
            throw error;
        }
    },

    // Reporte por rango de fechas
    async getReporteRangoFechas(fechaInicio, fechaFin) {
        try {
            const response = await axios.get(`${API_BASE_URL}/reportes/rango-fechas`, {
                params: {
                    fechaInicio: fechaInicio.toISOString().split('T')[0],
                    fechaFin: fechaFin.toISOString().split('T')[0]
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error al obtener reporte por rango:', error);
            throw error;
        }
    }
};

export default ReportesService;