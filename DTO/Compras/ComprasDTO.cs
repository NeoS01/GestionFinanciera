using System.ComponentModel.DataAnnotations;

namespace GestionFinanciera.DTO.Compras
{
    // ════════════════════════════════════════════════════════════════════════════
    // MOMENTO 1 — Verificar presupuesto
    // Compras nos llama antes de emitir una orden > 10,000 Bs.
    // POST /api/facturas/verificar-presupuesto/
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>JSON que Compras nos envía para consultar saldo disponible.</summary>
    public class VerificarPresupuestoRequest
    {
        [Required]
        public string CodigoOrden { get; set; } = string.Empty;      // "ORD-2026-003"

        [Required]
        public string Periodo { get; set; } = string.Empty;          // "2026-05"

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoRequerido { get; set; }

        [Required]
        public string AreaSolicitante { get; set; } = string.Empty;  // "Cuidados Críticos"
    }

    /// <summary>Respuesta cuando el presupuesto es suficiente.</summary>
    public class VerificarPresupuestoAprobado
    {
        public bool Aprobado { get; set; } = true;
        public decimal PresupuestoAutorizado { get; set; }
        public string CodigoValidacion { get; set; } = string.Empty; // "VAL-FIN-993"
    }

    /// <summary>Respuesta cuando el saldo es insuficiente.</summary>
    public class VerificarPresupuestoRechazado
    {
        public bool Aprobado { get; set; } = false;
        public string Motivo { get; set; } = string.Empty;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // MOMENTO 2 — Registrar pago de proveedor
    // Compras nos envía la factura una vez que el pedido llegó conforme.
    // POST /api/pagos/registrar/
    // ════════════════════════════════════════════════════════════════════════════

    /// <summary>JSON que Compras nos envía con los datos del pedido recibido.</summary>
    public class RegistrarPagoProveedorRequest
    {
        [Required]
        public string CodigoOrden { get; set; } = string.Empty;       // "ORD-2026-003"

        [Required]
        public string FacturaNumero { get; set; } = string.Empty;     // "FAC-001234"

        [Required]
        public string FacturaArchivoUrl { get; set; } = string.Empty; // URL al PDF

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoTotal { get; set; }

        [Required]
        public string Proveedor { get; set; } = string.Empty;         // "FarmaCorp S.A."

        [Required]
        public DateTime FechaRecepcion { get; set; }

        /// <summary>
        /// Compras solo envía esta solicitud cuando es true.
        /// Si llega false, rechazamos por protocolo.
        /// </summary>
        public bool RecibidoConforme { get; set; }
    }

    /// <summary>Respuesta exitosa al registrar el pago al proveedor.</summary>
    public class RegistrarPagoProveedorResponse
    {
        public bool PagoRegistrado { get; set; } = true;
        public string IdPago { get; set; } = string.Empty;  // "PAG-2026-099"
    }
}
