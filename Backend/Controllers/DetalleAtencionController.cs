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

        // GET: api/detalleatencion/ventas
        [HttpGet("ventas")]
        public async Task<IActionResult> GetVentas(int page = 1, int pageSize = 10)
        {
            var query = _context
                .Atencion.Include(a => a.Cliente)
                .ThenInclude(c => c.Rol)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .Where(a => a.DetalleAtencion.Any());

            var totalVentas = await query.CountAsync();

            var atenciones = await query
                .OrderByDescending(a => a.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking() // esto es para evitar nulls, EF puede rastrear objetos, con esto lo evitamos
                .ToListAsync();

            var atencionIds = atenciones.Select(a => a.Id).ToList();

            var pagos = await _context
                .Pagos.Where(p => atencionIds.Contains(p.AtencionId))
                .AsNoTracking()
                .ToListAsync();

            var ventas = atenciones
                .Select(a => new VentaDto
                {
                    AtencionId = a.Id,
                    ClienteId = a.ClienteId,
                    ClienteNombre = a.Cliente?.Nombre ?? "Cliente Desconocido",
                    Cliente = new UsuarioResumenDto
                    {
                        Id = a.Cliente?.Id ?? 0,
                        Nombre = a.Cliente?.Nombre ?? "Cliente Desconocido",
                    },
                    FechaAtencion = a.Fecha,
                    Detalles = a
                        .DetalleAtencion.Select(d => new DetalleVentaDto
                        {
                            ProductoServicioId = d.ProductoServicioId,
                            NombreProducto =
                                d.ProductoServicio?.Nombre ?? "Producto/Servicio borrado",
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Observacion = d.Observacion,
                        })
                        .ToList(),
                    Pagos = pagos
                        .Where(p => p.AtencionId == a.Id)
                        .Select(p => new PagoInfoDto
                        {
                            PagoId = p.Id,
                            MetodoPago = p.MetodoPago.ToString(),
                            Monto = p.Monto,
                            FechaPago = p.Fecha,
                        })
                        .ToList(),
                })
                .ToList();

            return Ok(
                new
                {
                    status = 200,
                    message = totalVentas > 0
                        ? "Ventas obtenidas correctamente."
                        : "No hay ventas registradas.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalVentas / pageSize),
                        currentPage = page,
                        pageSize,
                        total = totalVentas,
                    },
                    ventas = ventas,
                }
            );
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
