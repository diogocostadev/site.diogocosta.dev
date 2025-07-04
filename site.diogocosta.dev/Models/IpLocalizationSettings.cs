namespace site.diogocosta.dev.Models
{
    /// <summary>
    /// Configurações para a API de localização IP
    /// </summary>
    public class IpLocalizationSettings
    {
        /// <summary>
        /// URL base da API de localização IP
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Timeout em segundos para as requisições à API
        /// </summary>
        public int Timeout { get; set; } = 5;
    }
} 