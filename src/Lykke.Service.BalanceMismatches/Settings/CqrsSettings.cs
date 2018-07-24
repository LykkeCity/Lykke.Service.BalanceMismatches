using Lykke.SettingsReader.Attributes;
using System;

namespace Lykke.Service.BalanceMismatches.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnectionString { get; set; }

        public TimeSpan RetryDelay { get; set; }
    }
}
