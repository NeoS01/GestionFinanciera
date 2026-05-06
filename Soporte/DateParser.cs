using System.Globalization;

namespace GestionFinanciera.Soporte
{
    /// <summary>
    /// Utilidad para parsear fechas recibidas como query parameters.
    /// Acepta los formatos más comunes para evitar errores 500 por formato incorrecto.
    /// Formatos soportados: dd-MM-yyyy, yyyy-MM-dd, dd/MM/yyyy, yyyy/MM/dd, MM-dd-yyyy
    /// </summary>
    public static class DateParser
    {
        private static readonly string[] _formatos = new[]
        {
            "dd-MM-yyyy",
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "yyyy/MM/dd",
            "MM-dd-yyyy",
            "MM/dd/yyyy",
            "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ"
        };

        /// <summary>
        /// Intenta parsear la cadena de fecha en múltiples formatos.
        /// Retorna un DateTime UTC si tiene éxito, o null si ningún formato aplica.
        /// </summary>
        public static DateTime? TryParse(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            if (DateTime.TryParseExact(
                    valor,
                    _formatos,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime resultado))
            {
                return DateTime.SpecifyKind(resultado, DateTimeKind.Utc);
            }

            // Fallback: intento genérico
            if (DateTime.TryParse(valor, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime fallback))
            {
                return DateTime.SpecifyKind(fallback, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
