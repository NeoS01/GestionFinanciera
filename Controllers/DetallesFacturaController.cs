using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesFacturaController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public DetallesFacturaController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // GET: api/DetallesFactura/lista
        [HttpGet("lista")]
        public async Task<IActionResult> GetAll()
        {
            var data = await (from d in _context.DetalleFactura
                              where d.Estado != "Inactivo"
                              select new
                              {
                                  d.Codigo,
                                  d.CodigoServicio,
                                  d.Descripcion,
                                  d.PrecioUnitario,
                                  d.Subtotal
                              }).ToListAsync();
            return Ok(data);
        }

        // GET: api/DetallesFactura/porFactura/FAC-001
        [HttpGet("porFactura/{codigoFactura}")]
        public async Task<IActionResult> GetByFactura(string codigoFactura)
        {
            var data = await (from d in _context.DetalleFactura
                              join f in _context.Factura on d.Id_Factura equals f.Id
                              where f.Codigo == codigoFactura
                                    && d.Estado != "Inactivo"
                                    && f.Estado != "Inactivo"
                              select new
                              {
                                  CodigoDetalle = d.Codigo,
                                  CodigoFactura = f.Codigo,
                                  d.CodigoServicio,
                                  d.Descripcion,
                                  d.PrecioUnitario,
                                  d.Subtotal
                              }).ToListAsync();
            return Ok(data);
        }

        // POST: api/DetallesFactura/crear
        [HttpPost("crear")]
        public async Task<IActionResult> Create(
            string codigo,
            string codigoFactura,
            string codigoServicio,
            string descripcion,
            decimal precioUnitario,
            decimal subtotal)
        {
            bool existeCodigo = await _context.DetalleFactura.AnyAsync(d => d.Codigo == codigo);
            if (existeCodigo)
                return BadRequest($"El código '{codigo}' ya existe.");

            var factura = await (from f in _context.Factura
                                 where f.Codigo == codigoFactura && f.Estado != "Inactivo"
                                 select f).FirstOrDefaultAsync();
            if (factura == null)
                return BadRequest($"Factura '{codigoFactura}' no encontrada o inactiva.");

            var detalle = new DetalleFactura
            {
                Codigo = codigo,
                Id_Factura = factura.Id,
                CodigoServicio = codigoServicio,
                Descripcion = descripcion,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotal,
                Estado = "Activo"
            };

            _context.DetalleFactura.Add(detalle);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { mensaje = "Detalle creado con éxito." });
        }

        // DELETE: api/DetallesFactura/DET-001  (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Delete(string codigo)
        {
            var detalle = await (from d in _context.DetalleFactura
                                 where d.Codigo == codigo && d.Estado != "Inactivo"
                                 select d).FirstOrDefaultAsync();
            if (detalle == null)
                return NotFound($"Detalle '{codigo}' no encontrado.");

            detalle.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
