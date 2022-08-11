using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server;

public class Server : HealthServer {

    public Server(ILogger<Server> logger, EndPoint endPoint) : base(logger, endPoint) {
    }

    public override (bool, string?) GetStatus() {
        return (true, null);
    }
}