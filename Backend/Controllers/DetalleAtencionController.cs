using System.Text.Json;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleAtencionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DetalleAtencionController> _logger;

        public DetalleAtencionController(
            ApplicationDbContext context,
            ILogger<DetalleAtencionController> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/detalleatencion (Lista de detalles de atenciones)
        [HttpGet]
        public IActionResult GetDetallesAtencion(int page = 1, int pageSize = 10)
        {
            var totalDetalles = _context.DetalleAtencion.Count();
            var detalles = _context
                .DetalleAtencion.OrderBy(d => d.AtencionId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(
                new
                {
                    status = 200,
                    message = totalDetalles > 0
                        ? "Lista de detalles de atención obtenida correctamente."
                        : "No hay detalles de atención disponibles.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalDetalles / pageSize),
                        currentPage = page,
                        pageSize,
                        totalDetalles,
                    },
                    detalles = detalles ?? new List<DetalleAtencion>(), // esto es para evitar nulls
                }
            );
        }

        // GET: api/detalleatencion/{id} (Un detalle de atención específico)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalleAtencion(int id)
        {
            var detalleAtencion = await _context.DetalleAtencion.FindAsync(id);
            if (detalleAtencion == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El detalle de atención no existe.",
                    }
                );
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Detalle de atención encontrado.",
                    detalleAtencion,
                }
            );
        }

        // GET: api/detalleatencion/ventas (Obtener lista de ventas con filtros y paginación)
        [HttpGet("ventas")]
        public async Task<IActionResult> GetVentas(
            int page = 1,
            int pageSize = 10,
            string? clienteNombre = null,
            int? productoServicioId = null,
            string? productoNombre = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string ordenarPor = "fecha",
            bool ordenDescendente = true, // POR DEFECTO: ventas recientes arriba
            decimal? montoMin = null,
            decimal? montoMax = null
        )
        {
            try
            {
                _logger.LogInformation(
                    "Llamada a GetVentas con parámetros: page={Page}, pageSize={PageSize}, clienteNombre={ClienteNombre}, ordenarPor={OrdenarPor}, ordenDescendente={OrdenDescendente}",
                    page,
                    pageSize,
                    clienteNombre ?? "(null)",
                    ordenarPor,
                    ordenDescendente
                );

                var query = _context
                    .Atencion.Include(a => a.Cliente)
                    .Include(a => a.DetalleAtencion)
                    .ThenInclude(d => d.ProductoServicio)
                    .Where(a => a.DetalleAtencion.Any())
                    .AsQueryable();

                _logger.LogInformation("Consulta inicial construida.");

                // Filtro: nombre cliente
                if (!string.IsNullOrEmpty(clienteNombre))
                {
                    clienteNombre = clienteNombre.Trim().ToLower();
                    query = query.Where(a =>
                        a.Cliente != null
                        && a.Cliente.Nombre != null
                        && a.Cliente.Nombre.ToLower().Contains(clienteNombre)
                    );
                }

                // Filtro: ID producto
                if (productoServicioId.HasValue)
                {
                    query = query.Where(a =>
                        a.DetalleAtencion.Any(d => d.ProductoServicioId == productoServicioId.Value)
                    );
                }

                // Filtro: nombre producto
                if (!string.IsNullOrEmpty(productoNombre))
                {
                    productoNombre = productoNombre.Trim().ToLower();
                    query = query.Where(a =>
                        a.DetalleAtencion.Any(d =>
                            d.ProductoServicio != null
                            && d.ProductoServicio.Nombre != null
                            && d.ProductoServicio.Nombre.ToLower().Contains(productoNombre)
                        )
                    );
                }

                // Filtro: fecha desde/hasta
                if (fechaDesde.HasValue)
                {
                    query = query.Where(a => a.Fecha >= fechaDesde.Value.Date);
                }

                if (fechaHasta.HasValue)
                {
                    query = query.Where(a =>
                        a.Fecha <= fechaHasta.Value.Date.AddDays(1).AddTicks(-1)
                    );
                }

                // Log de la consulta SQL generada (EF Core 5+)
                _logger.LogInformation(
                    "Consulta SQL sin orden ni paginado: {Query}",
                    query.ToQueryString()
                );

                var totalFiltrado = await query.CountAsync();
                _logger.LogInformation("Total registros filtrados: {TotalFiltrado}", totalFiltrado);

                // Traemos solo la página sin ordenar (para evitar problemas con subconsultas en OrderBy)
                var atenciones = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation(
                    "Resultados obtenidos sin ordenar: {Count}",
                    atenciones.Count
                );

                // Ordenamiento en memoria
                switch (ordenarPor.ToLower())
                {
                    case "cliente":
                        atenciones = ordenDescendente
                            ? atenciones.OrderByDescending(a => a.Cliente?.Nombre ?? "").ToList()
                            : atenciones.OrderBy(a => a.Cliente?.Nombre ?? "").ToList();
                        break;

                    case "monto":
                        atenciones = ordenDescendente
                            ? atenciones
                                .OrderByDescending(a =>
                                    _context
                                        .Pagos.Where(p => p.AtencionId == a.Id)
                                        .Sum(p => p.Monto)
                                )
                                .ToList()
                            : atenciones
                                .OrderBy(a =>
                                    _context
                                        .Pagos.Where(p => p.AtencionId == a.Id)
                                        .Sum(p => p.Monto)
                                )
                                .ToList();
                        break;

                    default: // fecha
                        atenciones = ordenDescendente
                            ? atenciones.OrderByDescending(a => a.Fecha).ToList()
                            : atenciones.OrderBy(a => a.Fecha).ToList();
                        break;
                }

                _logger.LogInformation("Resultados ordenados en memoria.");

                var ventasFinal = new List<object>();

                foreach (var atencion in atenciones)
                {
                    var pagosVenta = await _context
                        .Pagos.Where(p => p.AtencionId == atencion.Id)
                        .Select(p => new PagoInfoDto
                        {
                            PagoId = p.Id,
                            MetodoPago = p.MetodoPago.ToString(),
                            Monto = p.Monto,
                            FechaPago = p.Fecha,
                        })
                        .ToListAsync();

                    var detalles = atencion
                        .DetalleAtencion.Select(d => new DetalleVentaDto
                        {
                            ProductoServicioId = d.ProductoServicioId,
                            NombreProducto =
                                d.ProductoServicio?.Nombre ?? "Producto/Servicio borrado",
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Observacion = d.Observacion,
                        })
                        .ToList();

                    var totalVenta = detalles.Sum(d => d.Subtotal);
                    var totalPagado = pagosVenta.Sum(p => p.Monto);

                    ventasFinal.Add(
                        new
                        {
                            atencionId = atencion.Id,
                            atencion.ClienteId,
                            ClienteNombre = atencion.Cliente?.Nombre ?? "Cliente Desconocido",
                            FechaAtencion = atencion.Fecha,
                            Detalles = detalles,
                            TotalVenta = totalVenta,
                            Pagos = pagosVenta,
                            MontoPagado = totalPagado,
                            EstadoPago = totalPagado >= totalVenta ? "Completo" : "Pendiente",
                        }
                    );
                }

                _logger.LogInformation(
                    "Ventas procesadas para respuesta: {Count}",
                    ventasFinal.Count
                );

                return Ok(
                    new
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
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetVentas");
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al obtener las ventas.",
                        details = ex.Message,
                    }
                );
            }
        }

        // GET: api/detalleatencion/ventas/{id}
        [HttpGet("ventas/{id}")]
        public async Task<IActionResult> GetVentaById(int id)
        {
            var atencion = await _context
                .Atencion.Include(a => a.Cliente)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (atencion == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La atención no existe.",
                    }
                );
            }

            var pagos = await _context.Pagos.Where(p => p.AtencionId == id).ToListAsync();

            var detalles = atencion
                .DetalleAtencion.Select(d => new DetalleVentaDto
                {
                    ProductoServicioId = d.ProductoServicioId,
                    NombreProducto = d.ProductoServicio?.Nombre ?? "Producto/Servicio borrado",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Observacion = d.Observacion,
                })
                .ToList();

            _logger.LogInformation(
                "DetalleAtencion enviados para la venta con Id {AtencionId}: {DetallesJson}",
                id,
                JsonSerializer.Serialize(detalles)
            );

            var venta = new VentaDto
            {
                AtencionId = atencion.Id,
                ClienteId = atencion.ClienteId,
                ClienteNombre = atencion.Cliente?.Nombre ?? "Cliente Desconocido",
                Cliente = new UsuarioResumenDto
                {
                    Id = atencion.Cliente?.Id ?? 0,
                    Nombre = atencion.Cliente?.Nombre ?? "Cliente Desconocido",
                },
                FechaAtencion = atencion.Fecha,
                Detalles = detalles,
                Pagos = pagos
                    .Select(p => new PagoInfoDto
                    {
                        PagoId = p.Id,
                        MetodoPago = p.MetodoPago.ToString(),
                        Monto = p.Monto,
                        FechaPago = p.Fecha,
                    })
                    .ToList(),
            };

            return Ok(
                new
                {
                    status = 200,
                    message = "Venta encontrada.",
                    venta,
                }
            );
        }

        // POST: api/detalleatencion (Registrar un nuevo detalle de atención)
        [HttpPost]
        public async Task<IActionResult> PostDetalleAtencion(DetalleAtencion detalleAtencion)
        {
            // Validar que `AtencionId` exista
            if (!await _context.Atencion.AnyAsync(a => a.Id == detalleAtencion.AtencionId))
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID de atención no es válido.",
                    }
                );
            }

            // Validar que `ProductoServicioId` exista
            if (
                !await _context.ProductosServicios.AnyAsync(p =>
                    p.Id == detalleAtencion.ProductoServicioId
                )
            )
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID del producto o servicio no es válido.",
                    }
                );
            }

            _context.DetalleAtencion.Add(detalleAtencion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDetalleAtencion),
                new { id = detalleAtencion.Id },
                new
                {
                    status = 201,
                    message = "Detalle de atención registrado correctamente.",
                    detalleAtencion,
                }
            );
        }

        // PUT: api/detalleatencion/{id} (Actualizar detalle de atención)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleAtencion(int id, DetalleAtencion detalleAtencion)
        {
            if (id != detalleAtencion.Id)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID en la URL no coincide con el ID del cuerpo de la solicitud.",
                    }
                );
            }

            _context.Entry(detalleAtencion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DetalleAtencion.Any(e => e.Id == id))
                {
                    return NotFound(
                        new
                        {
                            status = 404,
                            error = "Not Found",
                            message = "El detalle de atención no existe.",
                        }
                    );
                }
                throw;
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Detalle de atención actualizado correctamente.",
                    detalleAtencion,
                }
            );
        }

        // DELETE: api/detalleatencion/{id} (Eliminar detalle de atención)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleAtencion(int id)
        {
            var detalleAtencion = await _context.DetalleAtencion.FindAsync(id);
            if (detalleAtencion == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El detalle de atención no existe.",
                    }
                );
            }

            // 🔹 Verificar si el detalle de atención está vinculado a una atención existente
            var atencionExiste = await _context.Atencion.AnyAsync(a =>
                a.Id == detalleAtencion.AtencionId
            );
            if (!atencionExiste)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede eliminar el detalle de atención porque la atención asociada ya no existe.",
                        details = new
                        {
                            DetalleAtencionId = id,
                            Relacion = "Atencion",
                            Motivo = "Clave foránea inválida",
                        },
                    }
                );
            }

            // 🔹 Verificar si el detalle de atención está vinculado a un producto o servicio existente
            var productoServicioExiste = await _context.ProductosServicios.AnyAsync(p =>
                p.Id == detalleAtencion.ProductoServicioId
            );
            if (!productoServicioExiste)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede eliminar el detalle de atención porque el producto o servicio asociado ya no existe.",
                        details = new
                        {
                            DetalleAtencionId = id,
                            Relacion = "ProductoServicio",
                            Motivo = "Clave foránea inválida",
                        },
                    }
                );
            }

            _context.DetalleAtencion.Remove(detalleAtencion);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    status = 200,
                    message = "Detalle de atención eliminado correctamente.",
                    detalleAtencionEliminadoId = id,
                }
            );
        }
    }
}
