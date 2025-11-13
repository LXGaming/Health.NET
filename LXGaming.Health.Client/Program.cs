using System.Net;
using LXGaming.Health;
using LXGaming.Health.Client;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => {
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddConsole();
});

var logger = loggerFactory.CreateLogger<Client>();

string address;
if (args.Length == 0) {
    address = "127.0.0.1:4325";
} else if (args.Length == 1) {
    address = args[0];
} else {
    logger.LogError("Usage: LXGaming.Health.Client [Address]");
    return 3;
}

if (!IPEndPoint.TryParse(address, out var endPoint)) {
    logger.LogError("Failed to parse {Address} as an IPEndPoint", address);
    return 3;
}

logger.LogDebug("Connecting to {Endpoint}...", endPoint.ToString());

HealthClient client;
try {
    client = new Client(logger, endPoint);
} catch (Exception ex) {
    logger.LogError(ex, "Encountered an error while creating client");
    return 3;
}

try {
    client.Start();
} catch (Exception ex) {
    logger.LogError(ex, "Encountered an error while starting client");
    client.Dispose();
    return 3;
}

logger.LogDebug("Connected to {Endpoint}", endPoint.ToString());

try {
    var health = client.GetHealth();
    if (logger.IsEnabled(LogLevel.Debug)) {
        logger.LogDebug("{Status}", health.ToString());
    } else {
        Console.Write(health.Message);
    }

    return (byte) health.Status;
} catch (Exception ex) {
    logger.LogError(ex, "Encountered an error while getting health");
    return 3;
} finally {
    try {
        client.Stop();
    } catch (Exception ex) {
        logger.LogError(ex, "Encountered an error while shutting down client");
    } finally {
        client.Dispose();
    }
}