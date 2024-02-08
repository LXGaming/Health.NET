using System.Net;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Client;

public static class Program {

    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => {
        builder.AddConsole();
    });

    private static readonly ILogger<Client> Logger = Factory.CreateLogger<Client>();

    public static int Main(string[] args) {
        try {
            return Parser.Default.ParseArguments<Options>(args).MapResult(OnSuccess, OnFailure);
        } finally {
            Factory.Dispose();
        }
    }

    private static int OnSuccess(Options options) {
        EndPoint endPoint;
        try {
            var address = IPAddress.Parse(options.Address);
            endPoint = new IPEndPoint(address, options.Port);
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while parsing {Address}:{Port}", options.Address, options.Port);
            return 3;
        }

        HealthClient client;
        try {
            client = new Client(Logger, endPoint);
            client.Start();
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while starting client");
            return 3;
        }

        // Logger.LogInformation("Connected to {Endpoint}", endPoint);

        try {
            (bool state, string? message) status = client.GetStatus();
            Console.Write(status.message);
            return status.state ? Health.Healthy : Health.Unhealthy;
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while getting status");
            return 3;
        } finally {
            try {
                client.Stop();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while shutting down client");
            } finally {
                client.Dispose();
            }
        }
    }

    private static int OnFailure(IEnumerable<Error> errors) {
        return 3;
    }
}