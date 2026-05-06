using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArancelesController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public ArancelesController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Aranceles/lista
        [HttpGet("lista")]
        public async Task<IActionResult> GetAll()
        {
            var data = await (from a in _context.Arancel
                              where a.Estado != "Inactivo"
                              select new
                              {
                                  a.Codigo,
                                  a.CodigoServicio,
                                  a.NombreServicio,
                                  a.PrecioBase,
                                  a.PrecioConvenio
                              }).ToListAsync();
            return Ok(data);
        }

        // GET: api/Aranceles/buscar/ARA-001
        [HttpGet("buscar/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var arancel = await (from a in _context.Arancel
                                 where a.Codigo == codigo && a.Estado != "Inactivo"
                                 select new
                                 {
                                     a.Codigo,
                                     a.CodigoServicio,
                                     a.NombreServicio,
                                     a.PrecioBase,
                                     a.PrecioConvenio
                                 }).FirstOrDefaultAsync();
            if (arancel == null)
                return NotFound($"Arancel '{codigo}' no encontrado.");
            return Ok(arancel);
        }

        // GET: api/Aranceles/porConvenio/CON-001
        // JOIN 2 tablas: Arancel + Convenio (Requerimiento 6)
        [HttpGet("porConvenio/{codigoConvenio}")]
        public async Task<IActionResult> GetByConvenio(string codigoConvenio)
        {
            var data = await (from a in _context.Arancel
                              join c in _context.Convenio on a.Id_Convenio equals c.Id
                              where c.Codigo == codigoConvenio
                                    && a.Estado != "Inactivo"
                                    && c.Estado != "Inactivo"
                              select new
                              {
                                  Convenio = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura,
                                  a.CodigoServicio,
                                  a.NombreServicio,
                                  a.PrecioBase,
                                  a.PrecioConvenio
                              }).ToListAsync();

            return Ok(data);
        }

        // POST: api/Aranceles/crear  (5FN: opera solo con codigos)
        [HttpPost("crear")]
        public async Task<IActionResult> Create(
            string codigo,
            string codigoConvenio,
            string codigoServicio,
            string nombreServicio,
            decimal precioBase,
            decimal precioConvenio)
        {
            var convenio = await (from c in _context.Convenio
                                  where c.Codigo == codigoConvenio && c.Estado != "Inactivo"
                                  select c).FirstOrDefaultAsync();
            if (convenio == null)
                return BadRequest($"Convenio '{codigoConvenio}' no encontrado o inactivo.");

            bool existe = await _context.Arancel.AnyAsync(a => a.Codigo == codigo);
            if (existe)
                return BadRequest($"El código de arancel '{codigo}' ya existe.");

            bool duplicado = await _context.Arancel.AnyAsync(
                a => a.Id_Convenio == convenio.Id && a.CodigoServicio == codigoServicio && a.Estado != "Inactivo");
            if (duplicado)
                return BadRequest($"Ya existe un arancel para el servicio '{codigoServicio}' en el convenio '{codigoConvenio}'.");

            var arancel = new Arancel
            {
                Codigo = codigo,
                Id_Convenio = convenio.Id,
                CodigoServicio = codigoServicio,
                NombreServicio = nombreServicio,
                PrecioBase = precioBase,
                PrecioConvenio = precioConvenio,
                Estado = "Activo"
            };

            _context.Arancel.Add(arancel);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { mensaje = "Arancel creado con éxito." });
        }

        // PUT: api/Aranceles/ARA-001
        [HttpPut("{codigo}")]
        public async Task<IActionResult> Update(
            string codigo,
            string nombreServicio,
            decimal precioBase,
            decimal precioConvenio)
        {
            var arancel = await (from a in _context.Arancel
                                 where a.Codigo == codigo && a.Estado != "Inactivo"
                                 select a).FirstOrDefaultAsync();
            if (arancel == null)
                return NotFound($"Arancel '{codigo}' no encontrado.");

            arancel.NombreServicio = nombreServicio;
            arancel.PrecioBase = precioBase;
            arancel.PrecioConvenio = precioConvenio;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Arancel '{codigo}' actualizado." });
        }

        // DELETE: api/Aranceles/ARA-001  (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Delete(string codigo)
        {
            var arancel = await (from a in _context.Arancel
                                 where a.Codigo == codigo && a.Estado != "Inactivo"
                                 select a).FirstOrDefaultAsync();
            if (arancel == null)
                return NotFound($"Arancel '{codigo}' no encontrado.");

            arancel.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
