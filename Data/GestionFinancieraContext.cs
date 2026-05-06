using Microsoft.EntityFrameworkCore;
using GestionFinanciera.Dominio;

namespace GestionFinanciera.Data
{
    public class GestionFinancieraContext : DbContext
    {
        public GestionFinancieraContext(DbContextOptions<GestionFinancieraContext> options)
            : base(options)
        {
        }

        // ── Tablas existentes ────────────────────────────────────────────────
        public DbSet<Convenio>       Convenio       { get; set; } = default!;
        public DbSet<Arancel>        Arancel        { get; set; } = default!;
        public DbSet<Factura>        Factura        { get; set; } = default!;
        public DbSet<DetalleFactura> DetalleFactura { get; set; } = default!;
        public DbSet<Pago>           Pago           { get; set; } = default!;

        // ── Nuevas tablas: integración con Compras y Abastecimiento ──────────

        /// <summary>
        /// Presupuesto mensual asignado por Finanzas al departamento de Compras.
        /// Se crea/actualiza mediante POST /api/presupuesto-mensual/ (Momento 0).
        /// </summary>
        public DbSet<PresupuestoMensual> PresupuestoMensual { get; set; } = default!;

        /// <summary>
        /// Pagos a proveedores externos registrados cuando Compras
        /// envía una factura tras recibir un pedido conforme (Momento 2).
        /// </summary>
        public DbSet<PagoProveedor> PagoProveedor { get; set; } = default!;
    }
}
