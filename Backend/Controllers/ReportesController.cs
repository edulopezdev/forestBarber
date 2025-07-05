using backend.Data;
using backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(ApplicationDbContext context, ILogger<ReportesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // OPCIÓN 1: Reporte completo del día (recomendado)
        [HttpGet("dia-completo")]
        public async Task<IActionResult> GetReporteDiaCompleto(DateTime fecha)
        {
            try
            {
                // Turnos del día
                var turnosDelDia = await _context.Turno
                    .Where(t => t.FechaHora.Date == fecha.Date)
                    .CountAsync();

                // Atenciones del día con toda la información
                var atencionesDelDia = await _context.Atencion
                    .Include(a => a.Barbero)
                    .Include(a => a.Cliente)
                    .Include(a => a.DetalleAtencion)
                    .ThenInclude(d => d.ProductoServicio)
                    .Where(a => a.Fecha.Date == fecha.Date)
                    .ToListAsync();

                // Pagos del día
                var atencionIds = atencionesDelDia.Select(a => a.Id).ToList();
                var pagosDelDia = await _context.Pagos
                    .Where(p => atencionIds.Contains(p.AtencionId))
                    .ToListAsync();

                var reporte = new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    resumen = new
                    {
                        totalTurnos = turnosDelDia,
                        totalAtenciones = atencionesDelDia.Count,
                        totalFacturado = atencionesDelDia.Sum(a => a.Total),
                        totalPagado = pagosDelDia.Sum(p => p.Monto)
                    },
                    atenciones = atencionesDelDia.Select(a => new
                    {
                        atencionId = a.Id,
                        cliente = new { id = a.ClienteId, nombre = a.Cliente?.Nombre },
                        barbero = new { id = a.BarberoId, nombre = a.Barbero?.Nombre },
                        fecha = a.Fecha,
                        servicios = a.DetalleAtencion.Select(d => new
                        {
                            producto = d.ProductoServicio?.Nombre,
                            cantidad = d.Cantidad,
                            precio = d.PrecioUnitario,
                            subtotal = d.Cantidad * d.PrecioUnitario
                        }),
                        totalAtencion = a.Total,
                        pagos = pagosDelDia
                            .Where(p => p.AtencionId == a.Id)
                            .Select(p => new
                            {
                                metodo = p.MetodoPago.ToString(),
                                monto = p.Monto,
                                fecha = p.Fecha
                            })
                    })
                };

                return Ok(new
                {
                    status = 200,
                    message = "Reporte del día generado correctamente.",
                    data = reporte
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = "Error interno del servidor",
                    error = ex.Message
                });
            }
        }

        // OPCIÓN 2: Solo turnos del día
        [HttpGet("turnos-dia")]
        public async Task<IActionResult> GetTurnosPorDia(DateTime fecha)
        {
            var turnos = await _context.Turno
                .Include(t => t.Cliente)
                .Include(t => t.Barbero)
                .Include(t => t.EstadoTurno)
                .Where(t => t.FechaHora.Date == fecha.Date)
                .OrderBy(t => t.FechaHora)
                .ToListAsync();

            return Ok(new
            {
                status = 200,
                message = $"Turnos del {fecha:yyyy-MM-dd}",
                data = new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    totalTurnos = turnos.Count,
                    turnos = turnos.Select(t => new
                    {
                        id = t.Id,
                        fechaHora = t.FechaHora,
                        cliente = t.Cliente?.Nombre,
                        barbero = t.Barbero?.Nombre,
                        estado = t.EstadoTurno?.Nombre
                    })
                }
            });
        }

        // OPCIÓN 3: Solo atenciones del día
        [HttpGet("atenciones-dia")]
        public async Task<IActionResult> GetAtencionesPorDia(DateTime fecha)
        {
            var atenciones = await _context.Atencion
                .Include(a => a.Barbero)
                .Include(a => a.Cliente)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .Where(a => a.Fecha.Date == fecha.Date)
                .OrderBy(a => a.Fecha)
                .ToListAsync();

            return Ok(new
            {
                status = 200,
                message = $"Atenciones del {fecha:yyyy-MM-dd}",
                data = new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    totalAtenciones = atenciones.Count,
                    totalFacturado = atenciones.Sum(a => a.Total),
                    atenciones = atenciones.Select(a => new
                    {
                        id = a.Id,
                        cliente = a.Cliente?.Nombre,
                        barbero = a.Barbero?.Nombre,
                        fecha = a.Fecha,
                        total = a.Total,
                        servicios = a.DetalleAtencion.Select(d => new
                        {
                            servicio = d.ProductoServicio?.Nombre,
                            cantidad = d.Cantidad,
                            precio = d.PrecioUnitario
                        })
                    })
                }
            });
        }

        // OPCIÓN 4: Reporte por barbero del día
        [HttpGet("barbero-dia")]
        public async Task<IActionResult> GetReporteBarbero(int barberoId, DateTime fecha)
        {
            var atenciones = await _context.Atencion
                .Include(a => a.Cliente)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .Where(a => a.BarberoId == barberoId && a.Fecha.Date == fecha.Date)
                .ToListAsync();

            var barbero = await _context.Usuario.FindAsync(barberoId);

            return Ok(new
            {
                status = 200,
                message = $"Reporte de {barbero?.Nombre} del {fecha:yyyy-MM-dd}",
                data = new
                {
                    barbero = new { id = barberoId, nombre = barbero?.Nombre },
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    totalAtenciones = atenciones.Count,
                    totalGenerado = atenciones.Sum(a => a.Total),
                    atenciones = atenciones.Select(a => new
                    {
                        cliente = a.Cliente?.Nombre,
                        hora = a.Fecha.ToString("HH:mm"),
                        servicios = a.DetalleAtencion.Select(d => d.ProductoServicio?.Nombre),
                        total = a.Total
                    })
                }
            });
        }

        // OPCIÓN 5: Reporte de pagos del día
        [HttpGet("pagos-dia")]
        public async Task<IActionResult> GetPagosPorDia(DateTime fecha)
        {
            var pagos = await _context.Pagos
                .Include(p => p.Atencion)
                .ThenInclude(a => a.Cliente)
                .Where(p => p.Fecha.Date == fecha.Date)
                .OrderBy(p => p.Fecha)
                .ToListAsync();

            var totalPorMetodo = pagos
                .GroupBy(p => p.MetodoPago)
                .Select(g => new
                {
                    metodo = g.Key.ToString(),
                    total = g.Sum(p => p.Monto),
                    cantidad = g.Count()
                });

            return Ok(new
            {
                status = 200,
                message = $"Pagos del {fecha:yyyy-MM-dd}",
                data = new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    totalRecaudado = pagos.Sum(p => p.Monto),
                    totalPagos = pagos.Count,
                    porMetodo = totalPorMetodo,
                    pagos = pagos.Select(p => new
                    {
                        id = p.Id,
                        cliente = p.Atencion?.Cliente?.Nombre,
                        monto = p.Monto,
                        metodo = p.MetodoPago.ToString(),
                        hora = p.Fecha.ToString("HH:mm")
                    })
                }
            });
        }

        // OPCIÓN 6: Reporte consolidado por rango de fechas
        [HttpGet("rango-fechas")]
        public async Task<IActionResult> GetReporteRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "La fecha de inicio no puede ser mayor que la fecha fin."
                });
            }

            var atenciones = await _context.Atencion
                .Include(a => a.Barbero)
                .Include(a => a.Cliente)
                .Where(a => a.Fecha.Date >= fechaInicio.Date && a.Fecha.Date <= fechaFin.Date)
                .ToListAsync();

            var resumenPorDia = atenciones
                .GroupBy(a => a.Fecha.Date)
                .Select(g => new
                {
                    fecha = g.Key.ToString("yyyy-MM-dd"),
                    totalAtenciones = g.Count(),
                    totalFacturado = g.Sum(a => a.Total),
                    barberos = g.GroupBy(a => a.Barbero?.Nombre)
                        .Select(b => new
                        {
                            barbero = b.Key,
                            atenciones = b.Count(),
                            total = b.Sum(a => a.Total)
                        })
                });

            return Ok(new
            {
                status = 200,
                message = "Reporte por rango de fechas generado correctamente.",
                data = new
                {
                    fechaInicio = fechaInicio.ToString("yyyy-MM-dd"),
                    fechaFin = fechaFin.ToString("yyyy-MM-dd"),
                    resumenGeneral = new
                    {
                        totalAtenciones = atenciones.Count,
                        totalFacturado = atenciones.Sum(a => a.Total)
                    },
                    resumenPorDia
                }
            });
        }
    }
}