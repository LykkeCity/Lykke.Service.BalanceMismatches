using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainCashinDetector.Contract;
using Lykke.Job.BlockchainCashinDetector.Contract.Events;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.BalanceMismatches.Cqrs;
using Lykke.Service.BalanceMismatches.Settings;
using Lykke.SettingsReader;
using System.Collections.Generic;

namespace Lykke.Service.BalanceMismatches.Modules
{
    public class CqrsModule : Module
    {
        private static readonly string Self = "balance-mismatches";

        private readonly CqrsSettings _settings;

        public CqrsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.Nested(s => s.BalanceMismatchesService.Cqrs).CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };
            var rabbitMqEndpoint = rabbitMqSettings.Endpoint.ToString();

            // Command handlers
            builder.RegisterType<CashinProjection>();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    var messagingEngine = new MessagingEngine(
                        logFactory,
                        new TransportResolver(
                            new Dictionary<string, TransportInfo>
                            {
                                {
                                    "RabbitMq",
                                    new TransportInfo(
                                        rabbitMqEndpoint,
                                        rabbitMqSettings.UserName,
                                        rabbitMqSettings.Password, "None", "RabbitMq")
                                }
                            }),
                        new RabbitMqTransportFactory(logFactory));
                    return CreateEngine(ctx, messagingEngine, logFactory);
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;
            const string eventsRoute = "evets";

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    "messagepack",
                    environment: "lykke")),

                Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningEvents(typeof(CashinCompletedEvent))
                    .From(BlockchainCashinDetectorBoundedContext.Name)
                    .On(eventsRoute)
                    .WithProjection(typeof(CashinProjection), Self));
        }
    }
}
