using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PagoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/pago (Lista de pagos)
        [HttpGet]
        public IActionResult GetPagos(int page = 1, int pageSize = 10)
        {
            var totalPagos = _context.Pagos.Count();
            var pagos = _context
                .Pagos.Include(p => p.Atencion)
                .OrderBy(p => p.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(
                new
                {
                    status = 200,
                    message = totalPagos > 0
                        ? "Lista de pagos obtenida correctamente."
                        : "No hay pagos registrados.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalPagos / pageSize),
                        currentPage = page,
                        pageSize,
                        totalPagos,
                    },
                    pagos = pagos ?? new List<Pago>(),
                }
            );
        }

        // GET: api/pago/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPago(int id)
        {
            var pago = await _context
                .Pagos.Include(p => p.Atencion)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pago == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El pago no existe.",
                    }
                );
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Pago encontrado.",
                    pago,
                }
            );
        }

        // POST: api/pago
        [HttpPost]
        public async Task<IActionResult> PostPago(Pago pago)
        {
            // Validar monto mayor a 0
            if (pago.Monto <= 0)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El monto del pago debe ser mayor a 0.",
                    }
                );
            }

            // Validar que el AtencionId exista antes de registrar el pago
            var atencion = await _context.Atencion.FindAsync(pago.AtencionId);
            if (atencion == null)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "La atención asociada no existe.",
                    }
                );
            }
            
            // Validar que la venta no esté cerrada (asociada a un cierre de caja)
            if (atencion.CierreDiarioId != null)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede registrar un pago para una venta que ya está cerrada.",
                    }
                );
            }

            // Validar que MetodoPago sea válido
            if (!Enum.IsDefined(typeof(MetodoPago), pago.MetodoPago))
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "El método de pago no es válido.",
                    }
                );
            }

            // Agregar el pago a la base de datos
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            // Recargar el pago para incluir la entidad Atencion
            var pagoCreado = await _context
                .Pagos.Include(p => p.Atencion)
                .FirstOrDefaultAsync(p => p.Id == pago.Id);

            if (pagoCreado == null)
            {
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al guardar el pago.",
                    }
                );
            }

            return CreatedAtAction(
                nameof(GetPago),
                new { id = pagoCreado.Id },
                new
                {
                    status = 201,
                    message = "Pago creado correctamente.",
                    pago = pagoCreado,
                }
            );
        }

        // PUT: api/pago/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPago(int id, Pago pago)
        {
            if (id != pago.Id)
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

            _context.Entry(pago).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pagos.Any(e => e.Id == id))
                {
                    return NotFound(
                        new
                        {
                            status = 404,
                            error = "Not Found",
                            message = "El pago no existe.",
                        }
                    );
                }
                throw;
            }

            return Ok(
                new
                {
                    status = 200,
                    message = "Pago actualizado correctamente.",
                    pago,
                }
            );
        }

        // DELETE: api/pago/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePago(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound(
                    new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "El pago no existe.",
                    }
                );
            }
            
            // Verificar si la venta está cerrada
            var atencion = await _context.Atencion.FindAsync(pago.AtencionId);
            if (atencion != null && atencion.CierreDiarioId != null)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "No se puede eliminar un pago de una venta que ya está cerrada.",
                    }
                );
            }

            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    status = 200,
                    message = "Pago eliminado correctamente.",
                    pagoIdEliminado = id,
                }
            );
        }

        // ---------FILTROS--------- //

        // GET: api/pago/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public IActionResult GetPagosPorCliente(int clienteId, int page = 1, int pageSize = 10)
        {
            // Filtrar pagos por clienteId, asegurándonos de que Atencion no sea null
            var query = _context
                .Pagos.Include(p => p.Atencion)
                .Where(p => p.Atencion != null && p.Atencion.ClienteId == clienteId);

            var totalPagos = query.Count();

            // Obtener la lista de pagos paginada
            var pagos = query
                .OrderByDescending(p => p.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Devolver la respuesta
            return Ok(
                new
                {
                    status = 200,
                    message = totalPagos > 0
                        ? "Pagos del cliente obtenidos correctamente."
                        : "Este cliente no tiene pagos registrados.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalPagos / pageSize),
                        currentPage = page,
                        pageSize,
                        totalPagos,
                    },
                    pagos = pagos,
                }
            );
        }

        // GET: api/pago/fecha
        [HttpGet("fecha")]
        public IActionResult GetPagosPorFecha(
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 10
        )
        {
            // Verificar si las fechas son válidas
            if (startDate == null && endDate == null)
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "Se debe proporcionar al menos una fecha de inicio o de fin.",
                    }
                );
            }

            // Filtrar pagos según las fechas proporcionadas
            var query = _context.Pagos.Include(p => p.Atencion).AsQueryable();

            // Filtrar por fecha de inicio si está presente
            if (startDate.HasValue)
            {
                query = query.Where(p => p.Fecha >= startDate.Value);
            }

            // Filtrar por fecha de fin si está presente
            if (endDate.HasValue)
            {
                query = query.Where(p => p.Fecha <= endDate.Value);
            }

            var totalPagos = query.Count();

            // Obtener la lista de pagos paginada
            var pagos = query
                .OrderByDescending(p => p.Fecha) // Ordenar por fecha de forma descendente (puedes cambiarlo si es necesario)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Retornar la respuesta
            return Ok(
                new
                {
                    status = 200,
                    message = totalPagos > 0
                        ? "Pagos filtrados por fecha obtenidos correctamente."
                        : "No hay pagos registrados en el rango de fechas proporcionado.",
                    pagination = new
                    {
                        totalPages = (int)Math.Ceiling((double)totalPagos / pageSize),
                        currentPage = page,
                        pageSize,
                        totalPagos,
                    },
                    pagos = pagos,
                }
            );
        }

        // GET: api/pago/metodoPago/{metodoPago}
        [HttpGet("metodoPago/{metodoPago}")]
        public IActionResult GetPagosPorMetodoPago(
            string metodoPago,
            int page = 1,
            int pageSize = 10
        )
        {
            // Intentar convertir la cadena 'metodoPago' a su valor del enum MetodoPago
            if (Enum.TryParse(metodoPago, true, out MetodoPago metodoPagoEnum))
            {
                // Filtrar pagos por el valor del enum
                var query = _context
                    .Pagos.Include(p => p.Atencion)
                    .Where(p => p.MetodoPago == metodoPagoEnum);

                // Verificar si la consulta devuelve resultados
                var totalPagos = query.Count();

                // Si no hay pagos con el método de pago dado
                if (totalPagos == 0)
                {
                    return Ok(
                        new
                        {
                            status = 200,
                            message = "No se encontraron pagos con ese método de pago.",
                            pagination = new
                            {
                                totalPages = 0,
                                currentPage = page,
                                pageSize = pageSize,
                                totalPagos = totalPagos,
                            },
                            pagos = new List<Pago>(), // No hay pagos, se devuelve una lista vacía
                        }
                    );
                }

                // Cálculo de totalPages correctamente
                var totalPages = (int)Math.Ceiling((double)totalPagos / pageSize);

                // Asegurarse de que la página solicitada esté dentro de los límites
                if (page < 1)
                    page = 1;
                if (page > totalPages)
                    page = totalPages;

                // Limitar los pagos a la página y tamaño especificados
                var pagos = query
                    .OrderBy(p => p.Fecha)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Devolver la respuesta
                return Ok(
                    new
                    {
                        status = 200,
                        message = "Pagos filtrados por método de pago obtenidos correctamente.",
                        pagination = new
                        {
                            totalPages,
                            currentPage = page,
                            pageSize,
                            totalPagos,
                        },
                        pagos = pagos,
                    }
                );
            }
            else
            {
                return BadRequest(
                    new
                    {
                        status = 400,
                        error = "Bad Request",
                        message = "Método de pago no válido. Asegúrese de usar un valor válido como 'Efectivo', 'TarjetaCredito', etc.",
                    }
                );
            }
        }

        // GET: api/pago/facturacion
        [HttpGet("facturacion")]
        public IActionResult GetFacturacionPorFecha(DateTime fecha)
        {
            try
            {
                // Obtenemos los pagos del día (ignorando las horas)
                var pagosDelDia = _context
                    .Pagos.Include(p => p.Atencion)
                    .Where(p => p.Fecha.Date == fecha.Date)
                    .ToList();

                // Si no hay pagos, devolvemos respuesta adecuada
                if (!pagosDelDia.Any())
                {
                    return Ok(
                        new
                        {
                            status = 200,
                            message = "No se registraron pagos en la fecha indicada.",
                            facturacion = new { },
                        }
                    );
                }

                // Agrupamos los pagos por método de pago y sumamos los montos
                var porMetodo = pagosDelDia
                    .GroupBy(p => p.MetodoPago)
                    .Select(g => new { metodo = g.Key.ToString(), total = g.Sum(p => p.Monto) })
                    .ToList();

                // Calculamos el total de todos los pagos del día
                var facturacionTotal = pagosDelDia.Sum(p => p.Monto);

                // Obtenemos los detalles de atención para calcular productos y servicios
                var atencionIds = pagosDelDia.Select(p => p.AtencionId).Distinct().ToList();
                var detalles = _context
                    .DetalleAtencion.Include(d => d.ProductoServicio)
                    .Where(d => atencionIds.Contains(d.AtencionId))
                    .ToList();

                // Calculamos el total de servicios (productos no almacenables)
                var servicios = detalles
                    .Where(d =>
                        d.ProductoServicio != null
                        && d.ProductoServicio.EsAlmacenable.HasValue
                        && !d.ProductoServicio.EsAlmacenable.Value
                    )
                    .Sum(d => d.Cantidad * d.PrecioUnitario);

                // Calculamos el total de productos (productos almacenables)
                var productos = detalles
                    .Where(d =>
                        d.ProductoServicio != null
                        && d.ProductoServicio.EsAlmacenable.HasValue
                        && d.ProductoServicio.EsAlmacenable.Value
                    )
                    .Sum(d => d.Cantidad * d.PrecioUnitario);

                // Devolvemos los resultados, agregando la conclusión final
                return Ok(
                    new
                    {
                        status = 200,
                        message = "Facturación del día obtenida correctamente.",
                        fecha = fecha.ToString("yyyy-MM-dd"),
                        facturacionTotal = facturacionTotal,
                        servicios = new { total = servicios },
                        productos = new { total = productos },
                        metodoPago = porMetodo,
                        conclusion = new
                        {
                            totalEnCortes = servicios,
                            totalEnVentas = productos,
                            recaudacionDelDia = facturacionTotal, // Total real ingresado
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                // Si ocurre un error, devolvemos el error en el servidor
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        message = "Error interno en el servidor",
                        error = ex.Message,
                    }
                );
            }
        }

        // GET: api/pago/facturacion-mes
        [HttpGet("facturacion-mes")]
        public IActionResult GetFacturacionPorMes(int anio, int mes)
        {
            try
            {
                // Validar mes y año válidos
                if (mes < 1 || mes > 12)
                    return BadRequest(new { status = 400, message = "Mes inválido." });

                if (anio < 2000 || anio > DateTime.Now.Year)
                    return BadRequest(new { status = 400, message = "Año inválido." });

                // Rango de fechas para el mes especificado
                var fechaInicio = new DateTime(anio, mes, 1);
                var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

                // Obtenemos los pagos del mes (fecha entre inicio y fin)
                var pagosDelMes = _context
                    .Pagos.Include(p => p.Atencion)
                    .Where(p => p.Fecha.Date >= fechaInicio.Date && p.Fecha.Date <= fechaFin.Date)
                    .ToList();

                // Si no hay pagos, devolvemos respuesta adecuada
                if (!pagosDelMes.Any())
                {
                    return Ok(
                        new
                        {
                            status = 200,
                            message = "No se registraron pagos en el mes indicado.",
                            facturacion = new { },
                        }
                    );
                }

                // Agrupamos los pagos por método de pago y sumamos los montos
                var porMetodo = pagosDelMes
                    .GroupBy(p => p.MetodoPago)
                    .Select(g => new { metodo = g.Key.ToString(), total = g.Sum(p => p.Monto) })
                    .ToList();

                // Calculamos el total de todos los pagos del mes
                var facturacionTotal = pagosDelMes.Sum(p => p.Monto);

                // Obtenemos los detalles de atención para calcular productos y servicios
                var atencionIds = pagosDelMes.Select(p => p.AtencionId).Distinct().ToList();
                var totalAtenciones = atencionIds.Count; // 👈 Nuevo dato agregado

                var detalles = _context
                    .DetalleAtencion.Include(d => d.ProductoServicio)
                    .Where(d => atencionIds.Contains(d.AtencionId))
                    .ToList();

                // Calculamos el total de servicios (productos no almacenables)
                var servicios = detalles
                    .Where(d =>
                        d.ProductoServicio != null
                        && d.ProductoServicio.EsAlmacenable.HasValue
                        && !d.ProductoServicio.EsAlmacenable.Value
                    )
                    .Sum(d => d.Cantidad * d.PrecioUnitario);

                // Calculamos el total de productos (productos almacenables)
                var productos = detalles
                    .Where(d =>
                        d.ProductoServicio != null
                        && d.ProductoServicio.EsAlmacenable.HasValue
                        && d.ProductoServicio.EsAlmacenable.Value
                    )
                    .Sum(d => d.Cantidad * d.PrecioUnitario);

                // Devolvemos los resultados, agregando la conclusión final
                return Ok(
                    new
                    {
                        status = 200,
                        message = $"Facturación del mes {mes}/{anio} obtenida correctamente.",
                        mes = mes,
                        anio = anio,
                        facturacionTotal = facturacionTotal,
                        totalAtenciones = totalAtenciones, // 👈 Nuevo campo en la respuesta
                        servicios = new { total = servicios },
                        productos = new { total = productos },
                        metodoPago = porMetodo,
                        conclusion = new
                        {
                            totalEnCortes = servicios,
                            totalEnVentas = productos,
                            recaudacionDelMes = facturacionTotal,
                        },
                    }
                );
            }
            catch (Exception ex)
            {
                // Si ocurre un error, devolvemos el error en el servidor
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        message = "Error interno en el servidor",
                        error = ex.Message,
                    }
                );
            }
        }
    }
}
