using System.Net;
using LXGaming.Health.Models;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server;

public class Server(ILogger<Server> logger, EndPoint endPoint) : HealthServer(logger, endPoint) {

    public override Status GetStatus() {
        return new Status(true, null);
    }
}