using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionFinanciera.Dominio
{
    public class Arancel
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public int Id_Convenio { get; set; }

        public string CodigoServicio { get; set; } = string.Empty;

        public string NombreServicio { get; set; } = string.Empty;

        public decimal PrecioBase { get; set; }

        public decimal PrecioConvenio { get; set; }

        public string Estado { get; set; } = "Activo";

        [ForeignKey("Id_Convenio")]
        [JsonIgnore]
        public Convenio Convenio { get; set; } = null!;
    }
}
