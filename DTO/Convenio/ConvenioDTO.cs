namespace GestionFinanciera.DTO.Convenio
{
    public class ConvenioDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string NombreAseguradora { get; set; } = string.Empty;
        public string TipoCobertura { get; set; } = string.Empty;
        public decimal PorcentajeCoberturaBase { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
