using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;
using GestionFinanciera.DTO.Factura;
using Mapster;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public FacturasController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Facturas/lista
        [HttpGet("lista")]
        public async Task<IActionResult> GetAll()
        {
            var facturas = await _context.Factura
                .Where(f => f.Estado != "Inactivo")
                .ProjectToType<FacturaDTO>()
                .ToListAsync();
            return Ok(facturas);
        }

        // GET: api/Facturas/buscar/FAC-001
        [HttpGet("buscar/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var factura = await (from f in _context.Factura
                                 where f.Codigo == codigo && f.Estado != "Inactivo"
                                 select f).FirstOrDefaultAsync();
            if (factura == null)
                return NotFound($"Factura '{codigo}' no encontrada.");
            return Ok(factura.Adapt<FacturaDTO>());
        }

        // POST: api/Facturas/crear
        [HttpPost("crear")]
        public async Task<IActionResult> Create(
            string codigo,
            string codigoPaciente,
            DateTime fechaEmision,
            DateTime fechaVencimiento,
            decimal montoTotal,
            decimal montoCobertura,
            string? codigoConvenio = null)
        {
            bool existe = await _context.Factura.AnyAsync(f => f.Codigo == codigo);
            if (existe)
                return BadRequest($"El código de factura '{codigo}' ya existe.");

            int? idConvenio = null;
            if (!string.IsNullOrEmpty(codigoConvenio))
            {
                var convenio = await (from c in _context.Convenio
                                      where c.Codigo == codigoConvenio && c.Estado != "Inactivo"
                                      select c).FirstOrDefaultAsync();
                if (convenio == null)
                    return BadRequest($"Convenio '{codigoConvenio}' no encontrado o inactivo.");
                idConvenio = convenio.Id;
            }

            decimal montoPaciente = montoTotal - montoCobertura;

            var factura = new Factura
            {
                Codigo = codigo,
                CodigoPaciente = codigoPaciente,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                MontoTotal = montoTotal,
                MontoCobertura = montoCobertura,
                MontoPaciente = montoPaciente,
                Id_Convenio = idConvenio,
                EstadoFactura = "BORRADOR",
                Estado = "Activo"
            };

            _context.Factura.Add(factura);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { mensaje = "Factura creada con éxito." });
        }

        // PUT: api/Facturas/FAC-001
        [HttpPut("{codigo}")]
        public async Task<IActionResult> Update(
            string codigo,
            decimal montoTotal,
            decimal montoCobertura,
            string estadoFactura)
        {
            var factura = await (from f in _context.Factura
                                 where f.Codigo == codigo && f.Estado != "Inactivo"
                                 select f).FirstOrDefaultAsync();
            if (factura == null)
                return NotFound($"Factura '{codigo}' no encontrada.");

            factura.MontoTotal = montoTotal;
            factura.MontoCobertura = montoCobertura;
            factura.MontoPaciente = montoTotal - montoCobertura;
            factura.EstadoFactura = estadoFactura;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Factura '{codigo}' actualizada." });
        }

        // DELETE: api/Facturas/FAC-001  (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Delete(string codigo)
        {
            var factura = await (from f in _context.Factura
                                 where f.Codigo == codigo && f.Estado != "Inactivo"
                                 select f).FirstOrDefaultAsync();
            if (factura == null)
                return NotFound($"Factura '{codigo}' no encontrada.");

            factura.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
