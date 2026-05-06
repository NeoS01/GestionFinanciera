using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionFinanciera.Dominio
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public int Id_Paciente { get; set; }

        public string CodigoPaciente { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public decimal MontoTotal { get; set; }

        public decimal MontoCobertura { get; set; }

        public decimal MontoPaciente { get; set; }

        public string FolioExterno1 { get; set; } = string.Empty;

        public string FolioExterno2 { get; set; } = string.Empty;

        // FK opcional a Convenio (puede ser factura sin convenio)
        public int? Id_Convenio { get; set; }

        public string EstadoFactura { get; set; } = "BORRADOR"; // BORRADOR, EMITIDA, ENVIADA_ASEGURADORA, PAGADA_PARCIAL, PAGADA_TOTAL, RECHAZADA

        public string Estado { get; set; } = "Activo";

        [ForeignKey("Id_Convenio")]
        [JsonIgnore]
        public Convenio? Convenio { get; set; }

        [JsonIgnore]
        public List<DetalleFactura> Detalles { get; set; } = new();

        [JsonIgnore]
        public List<Pago> Pagos { get; set; } = new();
    }
}
