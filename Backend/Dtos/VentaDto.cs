namespace backend.Dtos
{
    public class VentaDto
    {
        public int AtencionId { get; set; }
        public int ClienteId { get; set; }

        public UsuarioResumenDto Cliente { get; set; } = new();

        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaAtencion { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new();
        public decimal TotalVenta => Detalles.Sum(d => d.Subtotal);

        public List<PagoInfoDto> Pagos { get; set; } = new();

        // Monto total pagado (suma de todos los pagos)
        public decimal MontoPagado => Pagos?.Sum(p => p.Monto) ?? 0;

        // Estado del pago basado en los pagos existentes
        public string EstadoPago =>
            Pagos == null || !Pagos.Any() ? "Sin pago"
            : MontoPagado < TotalVenta ? "Incompleto"
            : "Completo";
    }

    public class DetalleVentaDto
    {
        public int ProductoServicioId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
        public string? Observacion { get; set; }
    }

    public class PagoInfoDto
    {
        public int PagoId { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
    }

    public class MetodoPagoDetalleDto
    {
        public MetodoPago MetodoPago { get; set; }
        public decimal Monto { get; set; }
    }
}
