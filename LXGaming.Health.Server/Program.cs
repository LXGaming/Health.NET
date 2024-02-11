using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server;

public static class Program {

    private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => {
        builder.AddConsole();
    });

    private static readonly ILogger<Server> Logger = Factory.CreateLogger<Server>();

    public static int Main(string[] args) {
        string address;
        if (args.Length == 0) {
            address = "127.0.0.1:4325";
        } else if (args.Length == 1) {
            address = args[0];
        } else {
            Logger.LogError("Usage: LXGaming.Health.Server [Address]");
            return 1;
        }

        if (!IPEndPoint.TryParse(address, out var endPoint)) {
            Logger.LogError("Failed to parse {Address}", address);
            return 1;
        }

        HealthServer server;
        try {
            server = new Server(Logger, endPoint);
            server.Start();
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while starting server");
            return 1;
        }

        Logger.LogInformation("Listening on {Endpoint}", endPoint.ToString());

        try {
            Logger.LogInformation("Press Enter to continue...");
            Console.ReadLine();
            return 0;
        } catch (Exception) {
            return 1;
        } finally {
            try {
                server.Stop();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while shutting down server");
            } finally {
                server.Dispose();
            }
        }
    }
}