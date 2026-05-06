using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestionFinanciera.Dominio
{
    public class Convenio
    {
        [Key]
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string NombreAseguradora { get; set; } = string.Empty;

        public string TipoCobertura { get; set; } = string.Empty;

        public decimal PorcentajeCoberturaBase { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } = "Activo";

        // Navegacion
        [JsonIgnore]
        public List<Arancel> Aranceles { get; set; } = new();

        [JsonIgnore]
        public List<Factura> Facturas { get; set; } = new();
    }
}
