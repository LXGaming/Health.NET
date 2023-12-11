using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server;

public class Server(ILogger<Server> logger, EndPoint endPoint) : HealthServer(logger, endPoint) {

    public override (bool, string?) GetStatus() {
        return (true, null);
    }
}