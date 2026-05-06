using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionFinanciera.Dominio
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public int Id_Factura { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal Monto { get; set; }

        public string MetodoPago { get; set; } = string.Empty; // TARJETA_CREDITO, TRANSFERENCIA, EFECTIVO, DEBITO_AUTOMATICO

        public string ReferenciaBancaria { get; set; } = string.Empty;

        public string Pagado { get; set; } = string.Empty;

        public string Estado { get; set; } = "Activo";

        [ForeignKey("Id_Factura")]
        [JsonIgnore]
        public Factura Factura { get; set; } = null!;
    }
}
