using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Data;

namespace GestionFinanciera.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasMISController : ControllerBase
    {
        private readonly GestionFinancieraContext _context;

        public ConsultasMISController(GestionFinancieraContext context)
        {
            _context = context;
        }

        // 5 CONSULTAS GENÉRICAS

        // CG-1: Listado general con JOIN entre 2 tablas
        // Facturas activas con el nombre de su aseguradora (Factura + Convenio)
        [HttpGet("generica/listado-facturas-convenio")]
        public async Task<IActionResult> ListadoFacturasConConvenio()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id
                              where f.Estado != "Inactivo" && c.Estado != "Inactivo"
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  MontoTotal = f.MontoTotal,
                                  MontoCobertura = f.MontoCobertura,
                                  MontoPaciente = f.MontoPaciente,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura
                              }).ToListAsync();
            return Ok(data);
        }

        // CG-2: Agrupación con conteo (GROUP BY + COUNT)
        // Cantidad de facturas agrupadas por estado de factura
        [HttpGet("generica/conteo-facturas-por-estado")]
        public async Task<IActionResult> ConteoFacturasPorEstado()
        {
            var data = await (from f in _context.Factura
                              where f.Estado != "Inactivo"
                              group f by f.EstadoFactura into g
                              select new
                              {
                                  EstadoFactura = g.Key,
                                  CantidadFacturas = g.Count()
                              }).ToListAsync();
            return Ok(data);
        }

        // CG-3: Agrupación con suma (GROUP BY + SUM)
        // Total recaudado (suma de pagos) agrupado por método de pago
        [HttpGet("generica/total-recaudado-por-metodo-pago")]
        public async Task<IActionResult> TotalRecaudadoPorMetodoPago()
        {
            var data = await (from p in _context.Pago
                              where p.Estado != "Inactivo"
                              group p by p.MetodoPago into g
                              select new
                              {
                                  MetodoPago = g.Key,
                                  TotalRecaudado = g.Sum(x => x.Monto),
                                  CantidadPagos = g.Count()
                              }).OrderByDescending(x => x.TotalRecaudado)
                              .ToListAsync();
            return Ok(data);
        }

        // CG-4: Búsqueda filtrada por código
        // Buscar una factura por su código incluyendo datos del convenio
        [HttpGet("generica/buscar-factura/{codigoFactura}")]
        public async Task<IActionResult> BuscarFacturaPorCodigo(string codigoFactura)
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where f.Codigo == codigoFactura && f.Estado != "Inactivo"
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  FechaVencimiento = f.FechaVencimiento,
                                  MontoTotal = f.MontoTotal,
                                  MontoCobertura = f.MontoCobertura,
                                  MontoPaciente = f.MontoPaciente,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio",
                                  TipoCobertura = c != null ? c.TipoCobertura : "N/A"
                              }).FirstOrDefaultAsync();

            if (data == null)
                return NotFound($"Factura '{codigoFactura}' no encontrada.");
            return Ok(data);
        }

        // CG-5: Registros sin relación en otra tabla (NOT EXISTS)
        // Convenios que no tienen ninguna factura asignada
        [HttpGet("generica/convenios-sin-facturas")]
        public async Task<IActionResult> ConveniosSinFacturas()
        {
            var data = await (from c in _context.Convenio
                              where c.Estado != "Inactivo"
                                    && !_context.Factura.Any(f => f.Id_Convenio == c.Id && f.Estado != "Inactivo")
                              select new
                              {
                                  CodigoConvenio = c.Codigo,
                                  Aseguradora = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura,
                                  FechaInicio = c.FechaInicio,
                                  FechaFin = c.FechaFin
                              }).ToListAsync();
            return Ok(data);
        }

        // 10 CONSULTAS MIS DEL DIAGRAMA DE CASOS DE USO

        // MIS-1 (UC1): Listar facturas con convenio activo
        // JOIN Factura + Convenio — muestra solo facturas que tienen convenio vigente
        [HttpGet("mis/facturas-con-convenio-activo")]
        public async Task<IActionResult> FacturasConConvenioActivo()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id
                              where f.Estado != "Inactivo"
                                    && c.Estado != "Inactivo"
                                    && c.FechaFin >= DateTime.UtcNow
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura,
                                  PorcCobertura = c.PorcentajeCoberturaBase,
                                  MontoTotal = f.MontoTotal,
                                  MontoCobertura = f.MontoCobertura
                              }).ToListAsync();
            return Ok(data);
        }

        // MIS-2 (UC2): Consultar pagos por factura (JOIN 3 tablas)
        // Pago + Factura + Convenio — detalle completo de pagos con contexto de convenio
        [HttpGet("mis/pagos-por-factura/{codigoFactura}")]
        public async Task<IActionResult> PagosPorFactura(string codigoFactura)
        {
            var data = await (from p in _context.Pago
                              join f in _context.Factura on p.Id_Factura equals f.Id
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where f.Codigo == codigoFactura
                                    && p.Estado != "Inactivo"
                                    && f.Estado != "Inactivo"
                              select new
                              {
                                  CodigoPago = p.Codigo,
                                  FechaPago = p.FechaPago,
                                  MontoPagado = p.Monto,
                                  MetodoPago = p.MetodoPago,
                                  ReferenciaBancaria = p.ReferenciaBancaria,
                                  CodigoFactura = f.Codigo,
                                  MontoTotalFactura = f.MontoTotal,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio"
                              }).ToListAsync();
            return Ok(data);
        }

        // MIS-3 (UC3): Conteo de facturas por estado
        // GROUP BY EstadoFactura + Aseguradora — útil para panel de seguimiento
        [HttpGet("mis/conteo-facturas-por-estado-y-aseguradora")]
        public async Task<IActionResult> ConteoFacturasPorEstadoYAseguradora()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where f.Estado != "Inactivo"
                              group new { f, c } by new
                              {
                                  f.EstadoFactura,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio"
                              } into g
                              select new
                              {
                                  g.Key.EstadoFactura,
                                  g.Key.Aseguradora,
                                  CantidadFacturas = g.Count(),
                                  MontoTotalGrupo = g.Sum(x => x.f.MontoTotal)
                              }).OrderBy(x => x.EstadoFactura)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-4 (UC4): Total recaudado por convenio (GROUP BY + SUM + JOIN)
        // Cuánto ha recibido cada aseguradora en concepto de cobertura
        [HttpGet("mis/total-recaudado-por-convenio")]
        public async Task<IActionResult> TotalRecaudadoPorConvenio()
        {
            var data = await (from p in _context.Pago
                              join f in _context.Factura on p.Id_Factura equals f.Id
                              join c in _context.Convenio on f.Id_Convenio equals c.Id
                              where p.Estado != "Inactivo"
                                    && f.Estado != "Inactivo"
                                    && c.Estado != "Inactivo"
                              group new { p, c } by new
                              {
                                  c.Codigo,
                                  c.NombreAseguradora,
                                  c.TipoCobertura
                              } into g
                              select new
                              {
                                  CodigoConvenio = g.Key.Codigo,
                                  Aseguradora = g.Key.NombreAseguradora,
                                  TipoCobertura = g.Key.TipoCobertura,
                                  TotalRecaudado = g.Sum(x => x.p.Monto),
                                  CantidadPagos = g.Count()
                              }).OrderByDescending(x => x.TotalRecaudado)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-5 (UC5): Facturas sin pagos registrados (NOT EXISTS)
        // Facturas emitidas que aún no tienen ningún pago — cartera pendiente
        [HttpGet("mis/facturas-sin-pagos")]
        public async Task<IActionResult> FacturasSinPagos()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where f.Estado != "Inactivo"
                                    && !_context.Pago.Any(p => p.Id_Factura == f.Id && p.Estado != "Inactivo")
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  FechaVencimiento = f.FechaVencimiento,
                                  MontoTotal = f.MontoTotal,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio"
                              }).ToListAsync();
            return Ok(data);
        }

        // MIS-6 (UC6): Ranking de aseguradoras por monto cubierto
        // GROUP BY + SUM ordenado desc — indica cuál aseguradora genera más ingresos por cobertura
        [HttpGet("mis/ranking-aseguradoras-por-monto")]
        public async Task<IActionResult> RankingAseguradorasPorMonto()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id
                              where f.Estado != "Inactivo" && c.Estado != "Inactivo"
                              group new { f, c } by new
                              {
                                  c.Codigo,
                                  c.NombreAseguradora,
                                  c.TipoCobertura
                              } into g
                              select new
                              {
                                  CodigoConvenio = g.Key.Codigo,
                                  Aseguradora = g.Key.NombreAseguradora,
                                  TipoCobertura = g.Key.TipoCobertura,
                                  TotalCubierto = g.Sum(x => x.f.MontoCobertura),
                                  TotalFacturado = g.Sum(x => x.f.MontoTotal),
                                  CantidadFacturas = g.Count()
                              }).OrderByDescending(x => x.TotalCubierto)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-7 (UC7): Desglose de pagos por método (JOIN 3 tablas)
        // Pago + Factura + Convenio — distribución de métodos de pago por aseguradora
        [HttpGet("mis/desglose-pagos-por-metodo")]
        public async Task<IActionResult> DesglosePagosPorMetodo()
        {
            var data = await (from p in _context.Pago
                              join f in _context.Factura on p.Id_Factura equals f.Id
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where p.Estado != "Inactivo" && f.Estado != "Inactivo"
                              group new { p, c } by new
                              {
                                  p.MetodoPago,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio"
                              } into g
                              select new
                              {
                                  MetodoPago = g.Key.MetodoPago,
                                  Aseguradora = g.Key.Aseguradora,
                                  TotalPagado = g.Sum(x => x.p.Monto),
                                  CantidadTransacciones = g.Count()
                              }).OrderByDescending(x => x.TotalPagado)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-8 (UC8): Aranceles vigentes por convenio (JOIN 2 tablas)
        // Arancel + Convenio — listado de servicios cubiertos por cada aseguradora
        [HttpGet("mis/aranceles-vigentes-por-convenio")]
        public async Task<IActionResult> ArancelesVigentesPorConvenio()
        {
            var data = await (from a in _context.Arancel
                              join c in _context.Convenio on a.Id_Convenio equals c.Id
                              where a.Estado != "Inactivo"
                                    && c.Estado != "Inactivo"
                                    && c.FechaFin >= DateTime.UtcNow
                              select new
                              {
                                  CodigoArancel = a.Codigo,
                                  Aseguradora = c.NombreAseguradora,
                                  TipoCobertura = c.TipoCobertura,
                                  CodigoServicio = a.CodigoServicio,
                                  NombreServicio = a.NombreServicio,
                                  PrecioBase = a.PrecioBase,
                                  PrecioConvenio = a.PrecioConvenio,
                                  Descuento = a.PrecioBase - a.PrecioConvenio
                              }).OrderBy(x => x.Aseguradora)
                              .ThenBy(x => x.CodigoServicio)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-9 (UC9): Facturas vencidas sin cobrar (JOIN 2 tablas + filtro fecha)
        // Factura + Convenio — facturas cuya fecha de vencimiento ya pasó y siguen sin pagarse
        [HttpGet("mis/facturas-vencidas-sin-cobrar")]
        public async Task<IActionResult> FacturasVencidasSinCobrar()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              where f.Estado != "Inactivo"
                                    && f.FechaVencimiento < DateTime.UtcNow
                                    && f.EstadoFactura != "PAGADA_TOTAL"
                                    && !_context.Pago.Any(p => p.Id_Factura == f.Id && p.Estado != "Inactivo")
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  FechaVencimiento = f.FechaVencimiento,
                                  // ✅ LÍNEA CORREGIDA - Cambia esta línea
                                  DiasVencida = (DateTime.UtcNow - f.FechaVencimiento).Days,
                                  MontoTotal = f.MontoTotal,
                                  EstadoFactura = f.EstadoFactura,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio"
                              }).OrderBy(x => x.FechaVencimiento)
                              .ToListAsync();
            return Ok(data);
        }

        // MIS-10 (UC10): Reporte de cobertura vs pago paciente (JOIN 3 tablas)
        // Factura + Convenio + Pago — comparativa de cuánto cubrió la aseguradora vs cuánto pagó el paciente
        [HttpGet("mis/reporte-cobertura-vs-pago-paciente")]
        public async Task<IActionResult> ReporteCoberturaVsPagoPaciente()
        {
            var data = await (from f in _context.Factura
                              join c in _context.Convenio on f.Id_Convenio equals c.Id into convenioJoin
                              from c in convenioJoin.DefaultIfEmpty()
                              join p in _context.Pago on f.Id equals p.Id_Factura into pagosJoin
                              where f.Estado != "Inactivo"
                              select new
                              {
                                  CodigoFactura = f.Codigo,
                                  CodigoPaciente = f.CodigoPaciente,
                                  FechaEmision = f.FechaEmision,
                                  MontoTotal = f.MontoTotal,
                                  MontoCubiertoPorAseguradora = f.MontoCobertura,
                                  MontoPagadoPorPaciente = f.MontoPaciente,
                                  PorcentajeCubierto = f.MontoTotal > 0
                                      ? Math.Round((f.MontoCobertura / f.MontoTotal) * 100, 2)
                                      : 0,
                                  Aseguradora = c != null ? c.NombreAseguradora : "Sin convenio",
                                  TipoCobertura = c != null ? c.TipoCobertura : "N/A",
                                  EstadoFactura = f.EstadoFactura
                              }).OrderByDescending(x => x.MontoCubiertoPorAseguradora)
                              .ToListAsync();
            return Ok(data);
        }
    }
}
