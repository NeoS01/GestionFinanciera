using System.ComponentModel.DataAnnotations;

namespace GestionFinanciera.DTO.PresupuestoMensual
{
    // ─── REQUEST ────────────────────────────────────────────────────────────────

    /// <summary>
    /// JSON que Compras nos envía al inicio de mes (o cuando hay un ajuste).
    /// POST /api/presupuesto-mensual/
    /// </summary>
    public class PresupuestoMensualRequest
    {
        [Required]
        public string Periodo { get; set; } = string.Empty;          // "2026-05"

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoAsignado { get; set; }

        public string? Observaciones { get; set; }
    }

    // ─── RESPONSE ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Respuesta exitosa (HTTP 201) que devolvemos a Compras.
    /// </summary>
    public class PresupuestoMensualResponse
    {
        public string Periodo { get; set; } = string.Empty;
        public decimal MontoAsignado { get; set; }
        public decimal MontoDisponible { get; set; }
    }
}
