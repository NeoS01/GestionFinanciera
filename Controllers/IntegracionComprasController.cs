using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;
using GestionFinanciera.DTO.Compras;

namespace GestionFinanciera.Controllers
{
    /// <summary>
    /// Endpoints de integración con el microservicio de Compras y Abastecimiento.
    ///
    /// MOMENTO 1 — Compras consulta si hay saldo antes de emitir una orden grande.
    ///   POST /api/facturas/verificar-presupuesto/
    ///
    /// MOMENTO 2 — Compras nos envía la factura del proveedor para procesar el pago.
    ///   POST /api/pagos/registrar/
    /// </summary>
    [ApiController]
    public class IntegracionComprasController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public IntegracionComprasController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════════════════════════════════
        // MOMENTO 1
        // POST /api/facturas/verificar-presupuesto/
        //
        // Compras nos llama cuando una orden supera los 10,000 Bs.
        // Verificamos si el saldo mensual disponible alcanza.
        // Si aprobamos, descontamos el monto del saldo disponible y
        // devolvemos un código de validación que Compras guardará en
        // OrdenCompra.id_validacion_financiera.
        // ════════════════════════════════════════════════════════════════════════
        [HttpPost("api/facturas/verificar-presupuesto")]
        public async Task<IActionResult> VerificarPresupuesto(
            [FromBody] VerificarPresupuestoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Buscar el presupuesto del período indicado
            var presupuesto = await _context.PresupuestoMensual
                .FirstOrDefaultAsync(p => p.Periodo == request.Periodo
                                       && p.Estado != "Inactivo");

            if (presupuesto == null)
            {
                return Ok(new VerificarPresupuestoRechazado
                {
                    Aprobado = false,
                    Motivo   = $"No existe presupuesto registrado para el período '{request.Periodo}'."
                });
            }

            // ¿Hay saldo suficiente?
            if (presupuesto.MontoDisponible < request.MontoRequerido)
            {
                return Ok(new VerificarPresupuestoRechazado
                {
                    Aprobado = false,
                    Motivo   = $"Saldo insuficiente. Disponible este mes: {presupuesto.MontoDisponible:N2} Bs."
                });
            }

            // ── Aprobado: descontar el monto y generar código de validación ──
            presupuesto.MontoDisponible -= request.MontoRequerido;
            await _context.SaveChangesAsync();

            string codigoValidacion = GenerarCodigoValidacion();

            return Ok(new VerificarPresupuestoAprobado
            {
                Aprobado               = true,
                PresupuestoAutorizado  = request.MontoRequerido,
                CodigoValidacion       = codigoValidacion
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // MOMENTO 2
        // POST /api/pagos/registrar/
        //
        // Compras nos envía la factura del proveedor una vez que el pedido
        // llegó completo y en buen estado (recibido_conforme = true).
        // Registramos el pago pendiente para que Finanzas lo procese.
        // ════════════════════════════════════════════════════════════════════════
        [HttpPost("api/pagos/registrar")]
        public async Task<IActionResult> RegistrarPagoProveedor(
            [FromBody] RegistrarPagoProveedorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Guardia de protocolo: Compras no debería enviar si no llegó conforme,
            // pero lo validamos por si acaso.
            if (!request.RecibidoConforme)
            {
                return BadRequest(new
                {
                    error = "El pedido no fue recibido conforme. " +
                            "No se puede registrar el pago hasta resolver el problema con el proveedor."
                });
            }

            // Verificar que no se duplique la misma factura del proveedor
            bool yaRegistrada = await _context.PagoProveedor
                .AnyAsync(p => p.FacturaNumero == request.FacturaNumero
                            && p.Estado != "Inactivo");

            if (yaRegistrada)
            {
                return Conflict(new
                {
                    error = $"La factura '{request.FacturaNumero}' ya fue registrada anteriormente."
                });
            }

            // Generar código interno de pago
            string codigoPago = GenerarCodigoPago();

            var pagoProveedor = new PagoProveedor
            {
                CodigoPago        = codigoPago,
                CodigoOrden       = request.CodigoOrden,
                FacturaNumero     = request.FacturaNumero,
                FacturaArchivoUrl = request.FacturaArchivoUrl,
                MontoTotal        = request.MontoTotal,
                Proveedor         = request.Proveedor,
                FechaRecepcion    = request.FechaRecepcion,
                RecibidoConforme  = request.RecibidoConforme,
                EstadoPago        = "PENDIENTE",
                FechaRegistro     = DateTime.UtcNow,
                Estado            = "Activo"
            };

            _context.PagoProveedor.Add(pagoProveedor);
            await _context.SaveChangesAsync();

            return StatusCode(201, new RegistrarPagoProveedorResponse
            {
                PagoRegistrado = true,
                IdPago         = codigoPago
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        // GET /api/pagos/proveedores/
        // Lista interna de todos los pagos a proveedores pendientes / procesados.
        // ════════════════════════════════════════════════════════════════════════
        [HttpGet("api/pagos/proveedores")]
        public async Task<IActionResult> ListarPagosProveedores()
        {
            var pagos = await _context.PagoProveedor
                .Where(p => p.Estado != "Inactivo")
                .OrderByDescending(p => p.FechaRegistro)
                .Select(p => new
                {
                    p.CodigoPago,
                    p.CodigoOrden,
                    p.FacturaNumero,
                    p.MontoTotal,
                    p.Proveedor,
                    p.FechaRecepcion,
                    p.EstadoPago,
                    p.FechaRegistro
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // ────────────────────────────────────────────────────────────────────────
        // Helpers privados
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Genera un código de validación financiera único.
        /// Formato: VAL-FIN-{año}-{número secuencial de 4 dígitos}
        /// </summary>
        private string GenerarCodigoValidacion()
        {
            int anio = DateTime.UtcNow.Year;
            // Contamos cuántas validaciones hay este año para el secuencial
            int secuencial = _context.PagoProveedor.Count() + 1;
            return $"VAL-FIN-{anio}-{secuencial:D4}";
        }

        /// <summary>
        /// Genera un código de pago único.
        /// Formato: PAG-{año}-{número secuencial de 3 dígitos}
        /// </summary>
        private string GenerarCodigoPago()
        {
            int anio = DateTime.UtcNow.Year;
            int secuencial = _context.PagoProveedor.Count() + 1;
            return $"PAG-{anio}-{secuencial:D3}";
        }
    }
}
