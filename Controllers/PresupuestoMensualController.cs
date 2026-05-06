using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;
using GestionFinanciera.DTO.PresupuestoMensual;

namespace GestionFinanciera.Controllers
{
    /// <summary>
    /// MOMENTO 0 — Gestión Financiera recibe el presupuesto mensual de Compras.
    ///
    /// Compras nos llama al inicio de cada mes para asignarnos el monto disponible.
    /// Si hay un recorte o ampliación a mitad de mes, vuelven a llamar a este
    /// mismo endpoint y actualizamos el saldo disponible.
    ///
    /// Integración: Compras → POST /api/presupuesto-mensual/
    /// </summary>
    [Route("api/presupuesto-mensual")]
    [ApiController]
    public class PresupuestoMensualController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public PresupuestoMensualController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────────────
        // POST /api/presupuesto-mensual/
        // Compras nos asigna el presupuesto (o lo actualiza si hubo cambios).
        // ────────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AsignarOActualizar(
            [FromBody] PresupuestoMensualRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ¿Ya existe un presupuesto para este período?
            var existente = await _context.PresupuestoMensual
                .FirstOrDefaultAsync(p => p.Periodo == request.Periodo
                                       && p.Estado != "Inactivo");

            if (existente != null)
            {
                // Actualización de presupuesto (recorte o ampliación a mitad de mes)
                decimal diferencia = request.MontoAsignado - existente.MontoAsignado;
                existente.MontoAsignado   = request.MontoAsignado;
                existente.MontoDisponible = existente.MontoDisponible + diferencia;
                existente.Observaciones   = request.Observaciones ?? existente.Observaciones;
                existente.FechaRegistro   = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return StatusCode(201, new PresupuestoMensualResponse
                {
                    Periodo          = existente.Periodo,
                    MontoAsignado    = existente.MontoAsignado,
                    MontoDisponible  = existente.MontoDisponible
                });
            }

            // Primer registro del mes
            var nuevo = new PresupuestoMensual
            {
                Periodo          = request.Periodo,
                MontoAsignado    = request.MontoAsignado,
                MontoDisponible  = request.MontoAsignado,   // saldo completo al inicio
                Observaciones    = request.Observaciones ?? string.Empty,
                FechaRegistro    = DateTime.UtcNow,
                Estado           = "Activo"
            };

            _context.PresupuestoMensual.Add(nuevo);
            await _context.SaveChangesAsync();

            return StatusCode(201, new PresupuestoMensualResponse
            {
                Periodo         = nuevo.Periodo,
                MontoAsignado   = nuevo.MontoAsignado,
                MontoDisponible = nuevo.MontoDisponible
            });
        }

        // ────────────────────────────────────────────────────────────────────────
        // GET /api/presupuesto-mensual/{periodo}
        // Consulta interna: ¿cuánto queda disponible en un período?
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet("{periodo}")]
        public async Task<IActionResult> GetByPeriodo(string periodo)
        {
            var presupuesto = await _context.PresupuestoMensual
                .FirstOrDefaultAsync(p => p.Periodo == periodo
                                       && p.Estado != "Inactivo");

            if (presupuesto == null)
                return NotFound($"No hay presupuesto registrado para el período '{periodo}'.");

            return Ok(new PresupuestoMensualResponse
            {
                Periodo         = presupuesto.Periodo,
                MontoAsignado   = presupuesto.MontoAsignado,
                MontoDisponible = presupuesto.MontoDisponible
            });
        }

        // ────────────────────────────────────────────────────────────────────────
        // GET /api/presupuesto-mensual/
        // Lista todos los presupuestos registrados.
        // ────────────────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _context.PresupuestoMensual
                .Where(p => p.Estado != "Inactivo")
                .OrderByDescending(p => p.Periodo)
                .Select(p => new PresupuestoMensualResponse
                {
                    Periodo         = p.Periodo,
                    MontoAsignado   = p.MontoAsignado,
                    MontoDisponible = p.MontoDisponible
                })
                .ToListAsync();

            return Ok(lista);
        }
    }
}
