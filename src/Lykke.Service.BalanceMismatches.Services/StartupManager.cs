﻿using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Sdk;

namespace Lykke.Service.BalanceMismatches.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;

        public StartupManager(ICqrsEngine cqrsEngine)
        {
            _cqrsEngine = cqrsEngine;
        }

        public Task StartAsync()
        {
            _cqrsEngine.Start();

            return Task.CompletedTask;
        }
    }
}
