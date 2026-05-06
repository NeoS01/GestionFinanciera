namespace GestionFinanciera.DTO.Pago
{
    public class PagoDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string CodigoFactura { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string ReferenciaBancaria { get; set; } = string.Empty;
        public string Pagado { get; set; } = string.Empty;
    }
}
