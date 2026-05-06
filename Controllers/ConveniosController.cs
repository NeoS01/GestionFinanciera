using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;
using GestionFinanciera.Dominio;
using GestionFinanciera.DTO.Convenio;
using Mapster;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConveniosController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public ConveniosController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // GET: api/Convenios/lista
        [HttpGet("lista")]
        public async Task<IActionResult> GetAll()
        {
            var convenios = await _context.Convenio
                .Where(c => c.Estado != "Inactivo")
                .ProjectToType<ConvenioDTO>()
                .ToListAsync();
            return Ok(convenios);
        }

        // GET: api/Convenios/buscar/CON-001
        [HttpGet("buscar/{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var convenio = await (from c in _context.Convenio
                                  where c.Codigo == codigo && c.Estado != "Inactivo"
                                  select c).FirstOrDefaultAsync();
            if (convenio == null)
                return NotFound($"Convenio '{codigo}' no encontrado.");

            return Ok(convenio.Adapt<ConvenioDTO>());
        }

        // POST: api/Convenios/crear
        [HttpPost("crear")]
        public async Task<IActionResult> Create(
            string codigo,
            string nombreAseguradora,
            string tipoCobertura,
            decimal porcentajeCoberturaBase,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            bool existe = await _context.Convenio.AnyAsync(c => c.Codigo == codigo);
            if (existe)
                return BadRequest($"El código '{codigo}' ya existe.");

            var convenio = new Convenio
            {
                Codigo = codigo,
                NombreAseguradora = nombreAseguradora,
                TipoCobertura = tipoCobertura,
                PorcentajeCoberturaBase = porcentajeCoberturaBase,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Estado = "Activo"
            };

            _context.Convenio.Add(convenio);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { mensaje = "Convenio creado con éxito." });
        }

        // PUT: api/Convenios/CON-001
        [HttpPut("{codigo}")]
        public async Task<IActionResult> Update(
            string codigo,
            string nombreAseguradora,
            string tipoCobertura,
            decimal porcentajeCoberturaBase,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var convenio = await (from c in _context.Convenio
                                  where c.Codigo == codigo && c.Estado != "Inactivo"
                                  select c).FirstOrDefaultAsync();
            if (convenio == null)
                return NotFound($"Convenio '{codigo}' no encontrado.");

            convenio.NombreAseguradora = nombreAseguradora;
            convenio.TipoCobertura = tipoCobertura;
            convenio.PorcentajeCoberturaBase = porcentajeCoberturaBase;
            convenio.FechaInicio = fechaInicio;
            convenio.FechaFin = fechaFin;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = $"Convenio '{codigo}' actualizado." });
        }

        // DELETE: api/Convenios/CON-001  (Soft Delete)
        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Delete(string codigo)
        {
            var convenio = await (from c in _context.Convenio
                                  where c.Codigo == codigo && c.Estado != "Inactivo"
                                  select c).FirstOrDefaultAsync();
            if (convenio == null)
                return NotFound($"Convenio '{codigo}' no encontrado.");

            convenio.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
