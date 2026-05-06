namespace GestionFinanciera.DTO.Factura
{
    public class FacturaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string CodigoPaciente { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoCobertura { get; set; }
        public decimal MontoPaciente { get; set; }
        public string EstadoFactura { get; set; } = string.Empty;
    }
}
