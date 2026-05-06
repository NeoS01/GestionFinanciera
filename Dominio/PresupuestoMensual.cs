using System.ComponentModel.DataAnnotations;

namespace GestionFinanciera.Dominio
{
    /// <summary>
    /// Registra el presupuesto mensual asignado al departamento de Compras.
    /// Momento 0 de la integración con Compras y Abastecimiento.
    /// </summary>
    public class PresupuestoMensual
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Período en formato YYYY-MM (ej. "2026-05")</summary>
        public string Periodo { get; set; } = string.Empty;

        /// <summary>Monto total asignado por Finanzas para ese mes (en Bolivianos)</summary>
        public decimal MontoAsignado { get; set; }

        /// <summary>Saldo disponible (se descuenta con cada orden aprobada)</summary>
        public decimal MontoDisponible { get; set; }

        /// <summary>Contexto adicional del presupuesto (opcional)</summary>
        public string Observaciones { get; set; } = string.Empty;

        /// <summary>Fecha en que se registró o actualizó el presupuesto</summary>
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "Activo";
    }
}
