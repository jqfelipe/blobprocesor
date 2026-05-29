using Azure.Messaging.ServiceBus;
using BlobProcessor.Function.Models;
using BlobProcessor.Function.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.Configure<ServiceBusOptions>(context.Configuration.GetSection("ServiceBus"));
        services.AddSingleton<IFileProcessor, FileProcessor>();

        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration["ServiceBusConnection"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("The ServiceBusConnection setting is missing.");
            }

            return new ServiceBusClient(connectionString);
        });

        services.AddSingleton<IMessagePublisher, ServiceBusTopicPublisher>();
    })
    .Build();

await host.RunAsync();
