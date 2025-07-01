using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtencionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AtencionController> _logger;

        public AtencionController(ApplicationDbContext context, ILogger<AtencionController> logger)
        {
            _context = context;
            _logger = logger;
            _logger.LogInformation("AtencionController constructor called.");
        }

        // GET: api/atencion (Lista de atenciones)
        [HttpGet]
        public IActionResult GetAtenciones(int page = 1, int pageSize = 10)
        {
            var totalAtenciones = _context.Atencion.Count();
            _logger.LogInformation(
                "GetAtenciones called with page={Page}, pageSize={PageSize}",
                page,
                pageSize
            );
            _logger.LogInformation("Total atenciones in DB: {TotalAtenciones}", totalAtenciones);

            var atenciones = _context
                .Atencion.Include(a => a.Cliente)
                .Include(a => a.Barbero)
                .Include(a => a.DetalleAtencion)
                .ThenInclude(d => d.ProductoServicio)
                .OrderBy(a => a.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Returning {Count} atenciones", atenciones.Count);

            return Ok(
                new
                {
                    status = 200,
                    message = totalAtenciones > 0
                        ? "Lista de atenciones obtenida correctamente."
                        : "No hay atenciones disponibles.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalAtenciones / pageSize),
                        currentPage = page,
                        pageSize,
                        totalAtenciones,
                    },
                    atenciones = atenciones ?? new List<Atencion>(), // esto es para evitar nulls
                }
            );
        }

        // GET: api/atencion/{id} (Una atención específica)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAtencion(int id)
        {
            _logger.LogInformation("GetAtencion called with id={Id}", id);
            var atencion = await _context
                .Atencion.Include(a => a.Cliente) // Carga Cliente relacionado
                .Include(a => a.Barbero) // Carga Barbero relacionado
                .Include(a => a.DetalleAtencion) // Carga detalles de atención
                .ThenInclude(d => d.ProductoServicio) // Carga ProductoServicio en cada detalle
                .FirstOrDefaultAsync(a => a.Id == id);
            Console.WriteLine($"Cliente encontrado: {atencion?.Cliente?.Nombre}");

            if (atencion == null)
            {
                _logger.LogWarning("Atencion with id={Id} not found", id);
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La atención no existe.",
                    }
                );
            }
            _logger.LogInformation("Atencion with id={Id} found", id);
            return Ok(
                new
                {
                    status = 200,
                    message = "Atención encontrada.",
                    venta = new
                    {
                        atencionId = atencion.Id,
                        clienteId = atencion.ClienteId,
                        clienteNombre = atencion.Cliente?.Nombre,
                        fechaAtencion = atencion.Fecha,
                        detalles = atencion.DetalleAtencion.Select(d => new
                        {
                            productoServicioId = d.ProductoServicioId,
                            nombreProducto = d.ProductoServicio.Nombre,
                            cantidad = d.Cantidad,
                            precioUnitario = d.PrecioUnitario,
                            subtotal = d.Cantidad * d.PrecioUnitario,
                            observacion = d.Observacion, // Nuevo campo Observacion
                        }),
                        totalVenta = atencion.Total,
                        pagos = new List<object>(),
                        montoPagado = 0,
                        estadoPago = "Sin pago",
                    },
                }
            );
        }

        // POST: api/atencion (Registrar una nueva atención)
        [HttpPost]
        public async Task<IActionResult> PostAtencion([FromBody] CrearAtencionDto dto)
        {
            _logger.LogInformation("PostAtencion called with DTO: {@Dto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "ModelState invalid: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors)
                );

                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "Errores de validación",
                        details = ModelState
                            .Values.SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage),
                    }
                );
            }

            var barberoIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (
                string.IsNullOrEmpty(barberoIdStr) || !int.TryParse(barberoIdStr, out int barberoId)
            )
            {
                _logger.LogWarning(
                    "Usuario no autenticado o barberoId inválido: {BarberoIdStr}",
                    barberoIdStr
                );

                return Unauthorized(
                    new
                    {
                        status = 401,
                        error = "Unauthorized",
                        message = "Usuario no autenticado.",
                    }
                );
            }

            // Validar cliente
            var clienteExiste = await _context.Usuario.AnyAsync(u => u.Id == dto.ClienteId);
            if (!clienteExiste)
            {
                _logger.LogWarning("ClienteId no existe: {ClienteId}", dto.ClienteId);

                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = $"ClienteId {dto.ClienteId} no existe.",
                    }
                );
            }

            // Validar total
            if (dto.Total < 0)
            {
                _logger.LogWarning("Total inválido: {Total}", dto.Total);

                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El total debe ser un valor positivo.",
                    }
                );
            }

            // Validar detalles
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                _logger.LogWarning("Detalles de atención vacíos o nulos");

                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "La atención debe contener al menos un detalle.",
                    }
                );
            }

            foreach (var detalle in dto.Detalles)
            {
                if (detalle.Cantidad <= 0)
                {
                    _logger.LogWarning(
                        "Cantidad inválida en detalle: {Cantidad}",
                        detalle.Cantidad
                    );

                    return BadRequest(
                        new
                        {
                            status = 400,
                            error = "Bad Request",
                            message = "La cantidad en los detalles debe ser mayor que cero.",
                        }
                    );
                }

                if (detalle.PrecioUnitario < 0)
                {
                    _logger.LogWarning(
                        "PrecioUnitario inválido en detalle: {PrecioUnitario}",
                        detalle.PrecioUnitario
                    );

                    return BadRequest(
                        new
                        {
                            status = 400,
                            error = "Bad Request",
                            message = "El precio unitario en los detalles debe ser positivo.",
                        }
                    );
                }

                var productoExiste = await _context.ProductosServicios.AnyAsync(p =>
                    p.Id == detalle.ProductoServicioId
                );
                if (!productoExiste)
                {
                    _logger.LogWarning(
                        "ProductoServicioId no existe: {ProductoServicioId}",
                        detalle.ProductoServicioId
                    );

                    return BadRequest(
                        new
                        {
                            status = 400,
                            error = "Bad Request",
                            message = $"ProductoServicioId {detalle.ProductoServicioId} no existe.",
                        }
                    );
                }
            }

            var atencion = new Atencion
            {
                ClienteId = dto.ClienteId,
                BarberoId = barberoId,
                Fecha = dto.Fecha ?? DateTime.Now,
                Total = dto.Total,
                DetalleAtencion = dto
                    .Detalles.Select(d => new DetalleAtencion
                    {
                        ProductoServicioId = d.ProductoServicioId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Observacion = d.Observacion, //Nuevo campo Observacion
                    })
                    .ToList(),
            };

            try
            {
                _context.Atencion.Add(atencion);
                foreach (var detalle in atencion.DetalleAtencion)
                {
                    _logger.LogInformation(
                        "Detalle a guardar: ProductoServicioId={ProductoServicioId}, Cantidad={Cantidad}, PrecioUnitario={PrecioUnitario}, Observacion={Observacion}",
                        detalle.ProductoServicioId,
                        detalle.Cantidad,
                        detalle.PrecioUnitario,
                        detalle.Observacion ?? "null"
                    );
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Atención registrada correctamente con id={Id}",
                    atencion.Id
                );

                return CreatedAtAction(
                    nameof(GetAtencion),
                    new { id = atencion.Id },
                    new
                    {
                        status = 201,
                        message = "Atención registrada correctamente.",
                        atencion,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la atención");

                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al guardar la atención.",
                        details = ex.Message,
                    }
                );
            }
        }

        // PUT: api/atencion/{id} (Actualizar datos de una atención)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAtencion(int id, [FromBody] ActualizarAtencionDto dto)
        {
            _logger.LogInformation(
                "PutAtencion called with id={Id} and dto.ClienteId={ClienteId}",
                id,
                dto.ClienteId
            );

            _logger.LogInformation("DTO recibido en PUT /atencion/{Id}: {@Dto}", id, dto);

            if (id != dto.Id)
            {
                _logger.LogWarning(
                    "Id de URL {Id} no coincide con Id de body {BodyId}",
                    id,
                    dto.Id
                );
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ID en la URL no coincide con el ID del cuerpo de la solicitud.",
                    }
                );
            }

            var atencion = await _context.Atencion.FindAsync(id);

            if (atencion == null)
            {
                _logger.LogWarning("Atención con id={Id} no existe para actualizar", id);
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La atención no existe.",
                    }
                );
            }

            _logger.LogInformation("Buscando cliente con Id={ClienteId}", dto.ClienteId);

            var clienteExiste = await _context.Usuario.AnyAsync(u => u.Id == dto.ClienteId);

            _logger.LogInformation("Resultado de clienteExiste: {ClienteExiste}", clienteExiste);

            if (!clienteExiste)
            {
                _logger.LogWarning("ClienteId {ClienteId} no existe", dto.ClienteId);
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El ClienteId especificado no existe.",
                    }
                );
            }

            _logger.LogInformation("Buscando barbero con Id={BarberoId}", dto.BarberoId);

            var barberoExiste = await _context.Usuario.AnyAsync(u =>
                u.Id == dto.BarberoId && u.RolId == 2
            );

            _logger.LogInformation("Resultado de barberoExiste: {BarberoExiste}", barberoExiste);

            if (!barberoExiste)
            {
                _logger.LogWarning("BarberoId {BarberoId} no existe", dto.BarberoId);
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El BarberoId especificado no existe.",
                    }
                );
            }

            atencion.ClienteId = dto.ClienteId;
            atencion.BarberoId = dto.BarberoId;
            atencion.Fecha = dto.Fecha;
            atencion.Total = dto.Total;

            _context.Entry(atencion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Atención actualizada correctamente con id={Id}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Atencion.Any(e => e.Id == id))
                {
                    _logger.LogWarning("Atención con id={Id} no existe para actualizar", id);
                    return NotFound(
                        new
                        {
                            status = 404,
                            error = "Not Found",
                            message = "La atención no existe.",
                        }
                    );
                }
                throw;
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Atención actualizada correctamente.",
                    atencion,
                }
            );
        }

        // DELETE: api/atencion/{id} (Eliminar una atención)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAtencion(int id)
        {
            _logger.LogInformation("DeleteAtencion called with id={Id}", id);

            var atencion = await _context.Atencion.FindAsync(id);
            if (atencion == null)
            {
                _logger.LogWarning("Atención con id={Id} no existe para eliminar", id);

                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "La atención no existe.",
                    }
                );
            }

            // Verificar si la atención está vinculada en `detalle_atencion`
            var tieneDependencias = _context.DetalleAtencion.Any(d => d.AtencionId == id);
            if (tieneDependencias)
            {
                _logger.LogWarning(
                    "Atención con id={Id} no se puede eliminar por dependencias",
                    id
                );

                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede eliminar la atención porque está vinculada a registros en detalle de atención.",
                        details = new
                        {
                            AtencionId = id,
                            Relacion = "DetalleAtencion",
                            Motivo = "Restricción de clave foránea",
                        },
                    }
                );
            }

            _context.Atencion.Remove(atencion);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Atención eliminada correctamente con id={Id}", id);

            return Ok(
                new
                {
                    status = 200,
                    message = "Atención eliminada correctamente.",
                    atencionIdEliminado = id,
                }
            );
        }

        // GET: api/atencion/resumen-barbero (Obtener resumen de estadísticas de un barbero en un periodo específico)
        [HttpGet("resumen-barbero")]
        public IActionResult GetResumenBarbero(int barberoId, int mes = 0, int anio = 0)
        {
            _logger.LogInformation(
                "GetResumenBarbero called with barberoId={BarberoId}, mes={Mes}, anio={Anio}",
                barberoId,
                mes,
                anio
            );

            // Si no se pasa mes/anio, usar el actual
            if (mes == 0 || anio == 0)
            {
                var fechaActual = DateTime.Now;
                mes = fechaActual.Month;
                anio = fechaActual.Year;
                _logger.LogInformation("Fecha actual usada mes={Mes}, anio={Anio}", mes, anio);
            }

            // Filtrar atenciones del barbero en el periodo dado
            var query = _context.Atencion.Where(a => a.BarberoId == barberoId);

            // Filtrar por mes y año
            query = query.Where(a => a.Fecha.Month == mes && a.Fecha.Year == anio);

            // Obtener todas las atenciones del barbero en ese periodo
            var atenciones = query.ToList();

            // Calcular ingresos totales
            decimal totalIngresos = atenciones.Sum(a => a.Total);

            // Contar cantidad de atenciones
            int totalAtenciones = atenciones.Count;

            int totalCortes = atenciones.Count; // Ejemplo básico
            int totalServicios = 0;
            _logger.LogInformation(
                "Resumen calculado: totalAtenciones={TotalAtenciones}, totalIngresos={TotalIngresos}",
                totalAtenciones,
                totalIngresos
            );

            return Ok(
                new
                {
                    status = 200,
                    data = new
                    {
                        totalAtenciones,
                        totalCortes,
                        totalServicios,
                        totalIngresos,
                    },
                }
            );
        }
    }
}
