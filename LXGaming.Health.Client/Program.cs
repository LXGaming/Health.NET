using System.Net;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Client;

public static class Program {

    private static readonly ILogger<HealthClient> Logger = LoggerFactory.Create(builder => {
        builder.AddConsole();
    }).CreateLogger<HealthClient>();

    public static int Main(string[] args) {
        return Parser.Default.ParseArguments<Options>(args).MapResult(Success, Failure);
    }

    private static int Success(Options options) {
        EndPoint endPoint;
        try {
            var address = IPAddress.Parse(options.Address);
            endPoint = new IPEndPoint(address, options.Port);
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while parsing {Address}:{Port}", options.Address, options.Port);
            return 1;
        }

        HealthClient client;
        try {
            client = new HealthClient(Logger, endPoint);
            client.Start();
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while starting client");
            return 1;
        }

        // Logger.LogInformation("Connected to {Endpoint}", endPoint);

        try {
            return client.GetStatus() ? 0 : 1;
        } catch (Exception ex) {
            Logger.LogError(ex, "Encountered an error while getting status");
            return 1;
        } finally {
            try {
                client.Shutdown();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while shutting down client");
            }
        }
    }

    private static int Failure(IEnumerable<Error> errors) {
        return 1;
    }
}