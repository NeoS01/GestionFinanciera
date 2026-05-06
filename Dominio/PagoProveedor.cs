using System.ComponentModel.DataAnnotations;

namespace GestionFinanciera.Dominio
{
    /// <summary>
    /// Registra el pago a un proveedor externo cuando Compras nos envía
    /// la factura tras recibir un pedido conforme.
    /// Momento 2 de la integración con Compras y Abastecimiento.
    /// </summary>
    public class PagoProveedor
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Código interno generado (ej. "PAG-2026-099")</summary>
        public string CodigoPago { get; set; } = string.Empty;

        /// <summary>Código de la orden de compra que originó este pago</summary>
        public string CodigoOrden { get; set; } = string.Empty;

        /// <summary>Número de factura emitida por el proveedor</summary>
        public string FacturaNumero { get; set; } = string.Empty;

        /// <summary>URL al archivo PDF de la factura del proveedor</summary>
        public string FacturaArchivoUrl { get; set; } = string.Empty;

        /// <summary>Monto total a pagar al proveedor (en Bolivianos)</summary>
        public decimal MontoTotal { get; set; }

        /// <summary>Nombre del proveedor (ej. "FarmaCorp S.A.")</summary>
        public string Proveedor { get; set; } = string.Empty;

        /// <summary>Fecha en que Compras recibió físicamente el pedido</summary>
        public DateTime FechaRecepcion { get; set; }

        /// <summary>Confirmación de que el pedido llegó completo y en buen estado</summary>
        public bool RecibidoConforme { get; set; }

        /// <summary>Estado del pago: PENDIENTE, PROCESADO, RECHAZADO</summary>
        public string EstadoPago { get; set; } = "PENDIENTE";

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public string Estado { get; set; } = "Activo";
    }
}
