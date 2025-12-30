using System.Net;
using LXGaming.Health;
using LXGaming.Health.Server;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder => {
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger<Server>();

string address;
if (args.Length == 0) {
    address = "127.0.0.1:4325";
} else if (args.Length == 1) {
    address = args[0];
} else {
    logger.LogError("Usage: LXGaming.Health.Server [Address]");
    return 1;
}

if (!IPEndPoint.TryParse(address, out var endPoint)) {
    logger.LogError("Failed to parse {Address} as an IPEndPoint", address);
    return 1;
}

HealthServer server;
try {
    server = new Server(logger, endPoint);
} catch (Exception ex) {
    logger.LogError(ex, "Encountered an error while creating server");
    return 1;
}

try {
    server.Start();
} catch (Exception ex) {
    logger.LogError(ex, "Encountered an error while starting server");
    server.Dispose();
    return 1;
}

logger.LogInformation("Listening on {Endpoint}", endPoint.ToString());

try {
    logger.LogInformation("Press Enter to continue...");
    Console.ReadLine();
    return 0;
} catch (Exception) {
    return 1;
} finally {
    try {
        server.Stop();
    } catch (Exception ex) {
        logger.LogError(ex, "Encountered an error while shutting down server");
    } finally {
        server.Dispose();
    }
}