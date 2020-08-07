using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Diagnostics.EventFlow;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Serilog;
using Serilog;

class Program
{
    static async Task Main()
    {
        var pipeline = DiagnosticPipelineFactory.CreatePipeline("eventFlowConfig.json");

        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(XDocument.Load("serilog.xml").Root.Elements().ToDictionary(x => x.Attribute("Name").Value, x => x.Attribute("Value").Value));

        var logger = CreateLogger(pipeline, builder.Build());

        LogManager
            .Use<SerilogFactory>()
            .WithLogger(logger);

        Console.Title = "Samples.Logging.Default";
        #region ConfigureLogging
        var endpointConfiguration = new EndpointConfiguration("Samples.Logging.Default");
        // No config is required in version 5 and
        // higher since logging is enabled by default
        #endregion
        endpointConfiguration.UsePersistence<LearningPersistence>();
        endpointConfiguration.UseTransport<LearningTransport>();

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        var myMessage = new MyMessage();
        await endpointInstance.SendLocal(myMessage)
            .ConfigureAwait(false);
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }

    private static ILogger CreateLogger(DiagnosticPipeline pipeline, IConfiguration configuration) =>
            new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.EventFlow(pipeline)
                .CreateLogger();
}
