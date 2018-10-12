using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.BalanceMismatches.Client 
{
    /// <summary>
    /// BalanceMismatches client settings.
    /// </summary>
    public class BalanceMismatchesServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
