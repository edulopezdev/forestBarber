using System;
using System.Threading.Tasks;
using backend.Extensions;
using backend.Models;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CierreDiarioController : ControllerBase
    {
        private readonly ICierreDiarioService _cierreService;
        private readonly ILogger<CierreDiarioController> _logger;

        public CierreDiarioController(
            ICierreDiarioService cierreService,
            ILogger<CierreDiarioController> logger
        )
        {
            _cierreService = cierreService;
            _logger = logger;
        }

        // POST: api/cierreDiario
        [HttpPost]
        public async Task<IActionResult> CrearCierre([FromBody] CierreDiario cierre)
        {
            try
            {
                var nuevo = await _cierreService.CrearCierreAsync(cierre);
                return Ok(
                    new
                    {
                        status = 200,
                        message = "Cierre creado correctamente.",
                        cierre = nuevo,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cierre");
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al crear el cierre.",
                        details = ex.Message,
                    }
                );
            }
        }

        // GET: api/cierreDiario
        [HttpGet]
        public async Task<IActionResult> GetCierres()
        {
            try
            {
                var cierres = await _cierreService.ObtenerCierresAsync();
                return Ok(
                    new
                    {
                        status = 200,
                        message = "Cierres obtenidos correctamente.",
                        cierres,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cierres");
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al obtener los cierres.",
                        details = ex.Message,
                    }
                );
            }
        }

        // GET: api/cierreDiario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCierre(int id)
        {
            try
            {
                var cierre = await _cierreService.ObtenerCierrePorIdAsync(id);
                if (cierre == null)
                {
                    return NotFound(new { status = 404, message = "Cierre no encontrado." });
                }

                return Ok(
                    new
                    {
                        status = 200,
                        message = "Cierre obtenido correctamente.",
                        cierre,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cierre con id {id}");
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al obtener el cierre.",
                        details = ex.Message,
                    }
                );
            }
        }

        // PUT: api/cierreDiario/bloquear/{id}
        [HttpPut("bloquear/{id}")]
        public async Task<IActionResult> BloquearCierre(int id)
        {
            try
            {
                var result = await _cierreService.BloquearCierreAsync(id);
                if (!result)
                {
                    return NotFound(
                        new { status = 404, message = "Cierre no encontrado para bloquear." }
                    );
                }

                return Ok(new { status = 200, message = "Cierre bloqueado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al bloquear cierre con id {id}");
                return StatusCode(
                    500,
                    new
                    {
                        status = 500,
                        error = "Internal Server Error",
                        message = "Ocurrió un error al bloquear el cierre.",
                        details = ex.Message,
                    }
                );
            }
        }

        [HttpGet("por-fecha")]
        public async Task<IActionResult> ObtenerCierrePorFecha([FromQuery] DateTime fecha)
        {
            var cierre = await _cierreService.ObtenerCierrePorFechaAsync(fecha);
            if (cierre != null)
            {
                return Ok(new { status = 200, cierre });
            }

            var resumenDinamico = await _cierreService.ObtenerResumenDelDiaAsync(fecha);
            if (resumenDinamico == null)
            {
                return NotFound(
                    new { status = 404, message = "No se encontró cierre ni datos para esa fecha." }
                );
            }

            // Devolver resumen dinámico dentro de un objeto "cierre" para mantener estructura
            return Ok(new { status = 200, cierre = resumenDinamico });
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> ObtenerResumen([FromQuery] DateTime fecha)
        {
            var resumen = await _cierreService.ObtenerResumenDelDiaAsync(fecha);
            return Ok(new { status = 200, resumen });
        }

        public class CierreCajaRequest
        {
            public DateTime Fecha { get; set; }
            public string? Observaciones { get; set; }
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized(new { status = 401, message = "Usuario no autenticado." });

            var valido = await _cierreService.ValidarPasswordAsync(userId.Value, request.Password);
            if (!valido)
                return Unauthorized(new { status = 401, message = "Contraseña incorrecta." });

            // Verificar si ya existe un cierre para esta fecha
            var cierreExistente = await _cierreService.ObtenerCierrePorFechaAsync(request.Fecha);
            if (cierreExistente != null && cierreExistente.Cerrado)
                return BadRequest(
                    new { status = 400, message = "La caja para esta fecha ya está cerrada." }
                );

            // Obtener el resumen del día para la fecha especificada
            var resumen = await _cierreService.ObtenerResumenDelDiaAsync(request.Fecha) as dynamic;
            if (resumen == null)
                return BadRequest(
                    new
                    {
                        status = 400,
                        message = "No hay datos para cerrar la caja en esta fecha.",
                    }
                );

            // Crear el cierre con los datos del resumen
            var cierre = new CierreDiario
            {
                Fecha = request.Fecha,
                Observaciones = request.Observaciones,
                UsuarioId = userId.Value,
                TotalProductosVendidos = resumen.totalMontoProductos,
                TotalServiciosVendidos = resumen.totalMontoServicios,
                TotalVentasDia = resumen.totalIngresos,
                Cerrado = true,
            };

            // Agregar los pagos al cierre
            if (resumen.pagos != null)
            {
                foreach (var pago in resumen.pagos)
                {
                    // Convertir el metodoPago a string si es un objeto
                    string metodoPagoNombre;
                    if (pago.metodoPago is string)
                    {
                        metodoPagoNombre = (string)pago.metodoPago;
                    }
                    else
                    {
                        // Si es un objeto, intentar obtener su propiedad Nombre o ToString()
                        try
                        {
                            // Intentar acceder a una propiedad común como "Nombre" o "Descripcion"
                            metodoPagoNombre = pago.metodoPago.ToString();
                        }
                        catch
                        {
                            // Si falla, usar un valor genérico
                            metodoPagoNombre = "Método de pago";
                        }
                    }

                    cierre.Pagos.Add(
                        new CierreDiarioPago
                        {
                            MetodoPagoNombre = metodoPagoNombre,
                            Monto = pago.monto,
                        }
                    );
                }
            }

            // Guardar el cierre
            var cierreGuardado = await _cierreService.CrearCierreAsync(cierre);

            // Crear un objeto anónimo simplificado para evitar referencias circulares
            var cierreResponse = new
            {
                id = cierreGuardado.Id,
                fecha = cierreGuardado.Fecha,
                totalProductosVendidos = cierreGuardado.TotalProductosVendidos,
                totalServiciosVendidos = cierreGuardado.TotalServiciosVendidos,
                totalVentasDia = cierreGuardado.TotalVentasDia,
                observaciones = cierreGuardado.Observaciones,
                fechaCierre = cierreGuardado.FechaCierre,
                cerrado = cierreGuardado.Cerrado,
                usuarioId = cierreGuardado.UsuarioId,
                pagos = cierreGuardado
                    .Pagos.Select(p => new { metodoPago = p.MetodoPagoNombre, monto = p.Monto })
                    .ToList(),
            };

            return Ok(
                new
                {
                    status = 200,
                    message = "Caja cerrada correctamente",
                    cierre = cierreResponse,
                }
            );
        }
    }
}
