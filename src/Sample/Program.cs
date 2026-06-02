using System;
using System.Threading.Tasks;
using HandlerOrdering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.HandlerOrdering";
        var endpointConfiguration = new EndpointConfiguration("Samples.HandlerOrdering");
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport<LearningTransport>();
        #region config
        endpointConfiguration.ApplyInterfaceHandlerOrdering();
        #endregion

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddNServiceBusEndpoint(endpointConfiguration);
        var host = builder.Build();
        await host.StartAsync();
        var session = host.Services.GetRequiredService<IMessageSession>();
        var myMessage = new MyMessage();
        await session.SendLocal(myMessage);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await host.StopAsync();
    }
}