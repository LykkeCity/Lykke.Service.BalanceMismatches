using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainCashinDetector.Contract;
using Lykke.Job.BlockchainCashoutProcessor.Contract;
using Lykke.Job.BlockchainCashoutProcessor.Contract.Events;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.BalanceMismatches.Cqrs;
using Lykke.Service.BalanceMismatches.Settings;
using Lykke.SettingsReader;
using System.Collections.Generic;

namespace Lykke.Service.BalanceMismatches.Modules
{
    [UsedImplicitly]
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

            // Command handlers
            builder.RegisterType<CashOperationsProjection>();

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
                                        rabbitMqSettings.Endpoint.ToString(),
                                        rabbitMqSettings.UserName,
                                        rabbitMqSettings.Password,
                                        "None",
                                        "RabbitMq")
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
            const string eventsRoute = "evets";

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(
                    new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    SerializationFormat.MessagePack,
                    environment: "lykke")),

                Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(_settings.RetryDelay)

                    .ListeningEvents(typeof(Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                    .From(BlockchainCashinDetectorBoundedContext.Name).On(eventsRoute)
                    .WithProjection(typeof(CashOperationsProjection), BlockchainCashinDetectorBoundedContext.Name)

                    .ListeningEvents(typeof(CashinCompletedEvent), typeof(CashoutCompletedEvent))
                    .From(BlockchainCashoutProcessorBoundedContext.Name).On(eventsRoute)
                    .WithProjection(typeof(CashOperationsProjection), BlockchainCashoutProcessorBoundedContext.Name));
        }
    }
}
