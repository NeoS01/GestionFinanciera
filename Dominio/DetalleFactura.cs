using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionFinanciera.Dominio
{
    public class DetalleFactura
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public int Id_Factura { get; set; }

        public string CodigoServicio { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public string Estado { get; set; } = "Activo";

        [ForeignKey("Id_Factura")]
        [JsonIgnore]
        public Factura Factura { get; set; } = null!;
    }
}
