using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Client;

public static class Program {

    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => {
        builder.AddConsole();
    });

    private static readonly ILogger<Client> Logger = Factory.CreateLogger<Client>();

    public static int Main(string[] args) {
        string address;
        if (args.Length == 0) {
            address = "127.0.0.1:4325";
        } else if (args.Length == 1) {
            address = args[0];
        } else {
            Logger.LogError("Usage: LXGaming.Health.Client [Address]");
            return 1;
        }

        if (!IPEndPoint.TryParse(address, out var endPoint)) {
            Logger.LogError("Failed to parse {Address}", address);
            return 1;
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
            var status = client.GetStatus();
            Console.Write(status.Message);
            return status.State ? Health.Healthy : Health.Unhealthy;
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
}