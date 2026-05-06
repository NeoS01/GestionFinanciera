using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;
using GestionFinanciera.DTO.Pago;
using GestionFinanciera.Soporte;
using Mapster;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public PagosController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Pagos/lista
        [HttpGet("lista")]
        public async Task<IActionResult> GetAll()
        {
            var pagos = await _context.Pago
                .Where(p => p.Estado != "Inactivo")
                .ProjectToType<PagoDTO>()
                .ToListAsync();
            return Ok(pagos);
        }

        // GET: api/Pagos/buscar/PAG-001
        [HttpGet("buscar/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var pago = await (from p in _context.Pago
                              where p.Codigo == codigo && p.Estado != "Inactivo"
                              select p).FirstOrDefaultAsync();
            if (pago == null)
                return NotFound($"Pago '{codigo}' no encontrado.");
            return Ok(pago.Adapt<PagoDTO>());
        }

        // ============================================================
        // Requerimiento 6: JOIN 2 tablas (Pago + Factura)
        // GET: api/Pagos/consultaJoin2
        // ============================================================
        [HttpGet("consultaJoin2")]
        public async Task<IActionResult> ConsultaJoin2Tablas()
        {
            var data = await (from p in _context.Pago
                              join f in _context.Factura on p.Id_Factura equals f.Id
                              where p.Estado != "Inactivo" && f.Estado != "Inactivo"
                              select new
                              {
                                  CodigoPago = p.Codigo,
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaPago = p.FechaPago,
                                  MontoPagado = p.Monto,
                                  MetodoPago = p.MetodoPago,
                                  ReferenciaBancaria = p.ReferenciaBancaria,
                                  EstadoFactura = f.EstadoFactura
                              }).ToListAsync();
            return Ok(data);
        }

        // ============================================================
        // Requerimiento 7: JOIN 3 tablas (Pago + Factura + Convenio)
        // GET: api/Pagos/consultaJoin3
        // ============================================================
        [HttpGet("consultaJoin3")]
        public async Task<IActionResult> ConsultaJoin3Tablas()
        {
            var data = await (from p in _context.Pago
                              join f in _context.Factura on p.Id_Factura equals f.Id
                              join c in _context.Convenio on f.Id_Convenio equals c.Id
                              where p.Estado != "Inactivo"
                                    && f.Estado != "Inactivo"
                                    && c.Estado != "Inactivo"
                              select new
                              {
                                  CodigoPago = p.Codigo,
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  NombreAseguradora = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura,
                                  FechaPago = p.FechaPago,
                                  MontoPagado = p.Monto,
                                  MontoCubierto = f.MontoCobertura,
                                  MetodoPago = p.MetodoPago
                              }).ToListAsync();
            return Ok(data);
        }

        // POST: api/Pagos/crear
        [HttpPost("crear")]
        public async Task<IActionResult> Create(
            string codigo,
            string codigoFactura,
            string fechaPago,
            decimal monto,
            string metodoPago,
            string referenciaBancaria,
            string pagado)
        {
            var parsedFechaPago = DateParser.TryParse(fechaPago);
            if (parsedFechaPago is null)
                return BadRequest($"Formato de fechaPago no válido: '{fechaPago}'. Use dd-MM-yyyy o yyyy-MM-dd.");

            bool existeCodigo = await _context.Pago.AnyAsync(p => p.Codigo == codigo);
            if (existeCodigo)
                return BadRequest($"El código de pago '{codigo}' ya existe.");

            var factura = await (from f in _context.Factura
                                 where f.Codigo == codigoFactura && f.Estado != "Inactivo"
                                 select f).FirstOrDefaultAsync();
            if (factura == null)
                return BadRequest($"Factura '{codigoFactura}' no encontrada o inactiva.");

            var pago = new Pago
            {
                Codigo = codigo,
                Id_Factura = factura.Id,
                FechaPago = parsedFechaPago.Value,
                Monto = monto,
                MetodoPago = metodoPago,
                ReferenciaBancaria = referenciaBancaria,
                Pagado = pagado,
                Estado = "Activo"
            };

            _context.Pago.Add(pago);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { mensaje = "Pago registrado con éxito." });
        }

        // PUT: api/Pagos/PAG-001
        [HttpPut("{codigo}")]
        public async Task<IActionResult> Update(
            string codigo,
            decimal monto,
            string metodoPago,
            string referenciaBancaria,
            string pagado)
        {
            var pago = await (from p in _context.Pago
                              where p.Codigo == codigo && p.Estado != "Inactivo"
                              select p).FirstOrDefaultAsync();
            if (pago == null)
                return NotFound($"Pago '{codigo}' no encontrado.");

            pago.Monto = monto;
            pago.MetodoPago = metodoPago;
            pago.ReferenciaBancaria = referenciaBancaria;
            pago.Pagado = pagado;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Pago '{codigo}' actualizado." });
        }

        // DELETE: api/Pagos/PAG-001  (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Delete(string codigo)
        {
            var pago = await (from p in _context.Pago
                              where p.Codigo == codigo && p.Estado != "Inactivo"
                              select p).FirstOrDefaultAsync();
            if (pago == null)
                return NotFound($"Pago '{codigo}' no encontrado.");

            pago.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}