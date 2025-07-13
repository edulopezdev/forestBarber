using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentaService> _logger;

        public VentaService(ApplicationDbContext context, ILogger<VentaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<object> ObtenerVentasAsync(
            int page,
            int pageSize,
            string? clienteNombre,
            int? productoServicioId,
            string? productoNombre,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string ordenarPor,
            bool ordenDescendente,
            decimal? montoMin,
            decimal? montoMax,
            string? estadoPago
        )
        {
            // Base query con includes necesarios
            var baseQuery = _context
                .Atencion.Include(a => a.Cliente)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .Where(a => a.DetalleAtencion.Any());

            // Filtros según parámetros
            if (!string.IsNullOrEmpty(clienteNombre))
            {
                string lowerCliente = clienteNombre.Trim().ToLower();

                baseQuery = baseQuery.Where(a =>
                    a.Cliente != null
                    && a.Cliente.Nombre != null
                    && a.Cliente.Nombre.ToLower().Contains(lowerCliente)
                );
            }

            if (productoServicioId.HasValue)
            {
                baseQuery = baseQuery.Where(a =>
                    a.DetalleAtencion.Any(d => d.ProductoServicioId == productoServicioId.Value)
                );
            }

            if (!string.IsNullOrEmpty(productoNombre))
            {
                string lowerProd = productoNombre.Trim().ToLower();

                baseQuery = baseQuery.Where(a =>
                    a.DetalleAtencion.Any(d =>
                        d.ProductoServicio != null
                        && d.ProductoServicio.Nombre != null
                        && d.ProductoServicio.Nombre.ToLower().Contains(lowerProd)
                    )
                );
            }

            if (fechaDesde.HasValue)
                baseQuery = baseQuery.Where(a => a.Fecha >= fechaDesde.Value.Date);

            if (fechaHasta.HasValue)
                baseQuery = baseQuery.Where(a =>
                    a.Fecha <= fechaHasta.Value.Date.AddDays(1).AddTicks(-1)
                );

            // Traemos la lista con Pagos y Detalles en memoria para luego proyectar
            var lista = await baseQuery
                .Select(a => new
                {
                    Atencion = a,
                    Pagos = _context.Pagos.Where(p => p.AtencionId == a.Id).ToList(),
                    Detalles = a
                        .DetalleAtencion.Select(d => new DetalleVentaDto
                        {
                            ProductoServicioId = d.ProductoServicioId,
                            NombreProducto =
                                d.ProductoServicio != null
                                    ? (d.ProductoServicio.Nombre ?? "Sin nombre")
                                    : "Producto/Servicio borrado",
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Observacion = d.Observacion,
                        })
                        .ToList(),
                })
                .ToListAsync();

            // Proyección a forma final con nombres correctos y cálculo subtotal
            var query = lista
                .Select(a => new
                {
                    atencionId = a.Atencion.Id, // <-- nombre corregido aquí
                    a.Atencion.ClienteId,
                    ClienteNombre = a.Atencion.Cliente?.Nombre ?? "Cliente Desconocido",
                    FechaAtencion = a.Atencion.Fecha,
                    Detalles = a.Detalles,
                    TotalVenta = a.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                    Pagos = a
                        .Pagos.Select(p => new PagoInfoDto
                        {
                            PagoId = p.Id,
                            MetodoPago = p.MetodoPago.ToString(),
                            Monto = p.Monto,
                            FechaPago = p.Fecha,
                        })
                        .ToList(),
                })
                .Select(a => new
                {
                    a.atencionId,
                    a.ClienteId,
                    a.ClienteNombre,
                    a.FechaAtencion,
                    a.Detalles,
                    a.TotalVenta,
                    a.Pagos,
                    MontoPagado = a.Pagos.Sum(p => p.Monto),
                })
                .Select(a => new
                {
                    a.atencionId,
                    a.ClienteId,
                    a.ClienteNombre,
                    a.FechaAtencion,
                    a.Detalles,
                    a.TotalVenta,
                    a.Pagos,
                    a.MontoPagado,
                    EstadoPago = a.MontoPagado >= a.TotalVenta ? "Completo" : "Pendiente",
                })
                .AsQueryable();

            // Aplicar filtros adicionales sobre la lista proyectada
            if (montoMin.HasValue)
                query = query.Where(a => a.TotalVenta >= montoMin.Value);

            if (montoMax.HasValue)
                query = query.Where(a => a.TotalVenta <= montoMax.Value);

            if (!string.IsNullOrEmpty(estadoPago))
                query = query.Where(a => a.EstadoPago.ToLower() == estadoPago.ToLower());

            if (string.IsNullOrEmpty(ordenarPor))
                ordenarPor = "fecha";

            // Ordenamiento
            query = ordenarPor.ToLower() switch
            {
                "cliente" => ordenDescendente
                    ? query.OrderByDescending(a => a.ClienteNombre)
                    : query.OrderBy(a => a.ClienteNombre),

                "monto" => ordenDescendente
                    ? query.OrderByDescending(a => a.TotalVenta)
                    : query.OrderBy(a => a.TotalVenta),

                _ => ordenDescendente
                    ? query.OrderByDescending(a => a.FechaAtencion)
                    : query.OrderBy(a => a.FechaAtencion),
            };

            var totalFiltrado = query.Count();

            // Paginación
            var ventasFinal = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new
            {
                status = 200,
                message = ventasFinal.Any()
                    ? "Ventas obtenidas correctamente."
                    : "No hay ventas registradas.",
                pagination = new
                {
                    totalPages = (int)Math.Ceiling((double)totalFiltrado / pageSize),
                    currentPage = page,
                    pageSize,
                    total = totalFiltrado,
                },
                ventas = ventasFinal,
            };
        }
    }
}
