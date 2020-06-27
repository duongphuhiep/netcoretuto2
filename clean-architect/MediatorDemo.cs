using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using poc.client.behaviors;
using poc.client.handlers;
using Scrutor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolsPack.NLog;

namespace poc.client
{
    namespace handlers
    {
        public class FooRequest : IRequest<string> 
        {
            public FooRequest(string id)
            {
                Id = id;
            }
            public string Id { get; }
        }
        public class FooHandler1 : IRequestHandler<FooRequest, string>
        {
            private static int IdCounter;
            public FooHandler1()
            {
                Id = IdCounter++;
            }
            public Type RuntimeType => typeof(FooHandler1);
            public int Id { get; }
            public Task<string> Handle(FooRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult($"{RuntimeType.Name} #{Id} (requestId: {request.Id})");
            }
        }
        public class FooHandler2 : IRequestHandler<FooRequest, string>
        {
            private static int IdCounter;
            public FooHandler2()
            {
                Id = IdCounter++;
            }
            public Type RuntimeType => typeof(FooHandler2);
            public int Id { get; }
            public Task<string> Handle(FooRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult($"{RuntimeType.Name} #{Id} (requestId: {request.Id})");
            }
        }
        public class BarNotification : INotification
        {
            public BarNotification(string id)
            {
                Id = id;
            }
            public string Id { get; }
        }
        public class BarHandler1 : INotificationHandler<BarNotification>
        {
            private static int IdCounter;
            public BarHandler1()
            {
                Id = IdCounter++;
            }
            public Type RuntimeType => typeof(BarHandler1);
            public int Id { get; }
            
            public Task Handle(BarNotification notification, CancellationToken cancellationToken)
            {
                Console.WriteLine($"{RuntimeType.Name} #{Id} (notificationId: {notification.Id})");
                return Task.CompletedTask;
            }
        }
        public class BarHandler2 : INotificationHandler<BarNotification>
        {
            private static int IdCounter;
            public BarHandler2()
            {
                Id = IdCounter++;
            }
            public Type RuntimeType => typeof(BarHandler2);
            public int Id { get; }

            public Task Handle(BarNotification notification, CancellationToken cancellationToken)
            {
                Console.WriteLine($"{RuntimeType.Name} #{Id} (notificationId: {notification.Id})");
                return Task.CompletedTask;
            }
        }
    }

    namespace behaviors
    {
        public class LoggingMiddleware<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            private readonly ILogger<IPipelineBehavior<TRequest, TResponse>> _logger;

            public LoggingMiddleware(ILogger<IPipelineBehavior<TRequest, TResponse>> logger)
            {
                _logger = logger;
            }

            public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
            {
                _logger.LogInformation($"LoggingMiddleware: Before calling request {typeof(TRequest).Name}");
                var response = await next();
                _logger.LogInformation($"LoggingMiddleware: After getting response {typeof(TResponse).Name}");

                return response;
            }
        }
        public class FooMiddleware<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
            {
                Console.WriteLine($"FooMiddleware: Before calling request {typeof(TRequest).Name}");
                var response = await next();
                Console.WriteLine($"FooMiddleware: After getting response  {typeof(TResponse).Name}");

                return response;
            }
        }
    }

    public static class MediatorDemo
    {
        private static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddNLog(LogQuickConfig.SetupFileAndConsole("./logs/mediator-demo.log"));
            LogQuickConfig.UseNewtonsoftJson();
        });

        public static async Task Play001Async()
        {
            //Scan all the handler in the Assembly and register them to the serviceProvider
            IServiceProviderFactory<IServiceCollection> serviceProviderFactory = new DefaultServiceProviderFactory();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Scan(scan => scan.FromCallingAssembly()
                .AddClasses(classes => classes.InExactNamespaceOf<FooHandler1>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            //Register Bahaviors: the registering order is important: in this example the FooMiddleware is invoke before the LoggingMiddleware in the Pipeline
            serviceCollection.AddScoped(typeof(IPipelineBehavior<FooRequest, string>), typeof(FooMiddleware<FooRequest, string>));
            serviceCollection.AddScoped(typeof(IPipelineBehavior<FooRequest, string>), typeof(LoggingMiddleware<FooRequest, string>));

            ILogger<IPipelineBehavior<FooRequest, string>> log = loggerFactory.CreateLogger<LoggingMiddleware<FooRequest, string>>();
            serviceCollection.AddSingleton(log);

            //serviceCollection.AddScoped<FooExceptionHandler>();
            IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);

            //Create a mediator which use the serviceProvider to create the service
            IMediator mediator = new Mediator(t => serviceProvider.GetService(t));

            Console.WriteLine(await mediator.Send(new handlers.FooRequest("foo-1")));
            //Console.WriteLine(await mediator.Send(new handlers.FooRequest("foo-2")));

            //await mediator.Publish(new BarNotification("bar-1"));
        }
    }
}
