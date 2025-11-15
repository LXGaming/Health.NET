using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server;

public class Server(ILogger<Server> logger, EndPoint endPoint) : HealthServer(logger, endPoint) {

    public HealthResult Health { get; set; } = new(HealthStatus.Healthy, Guid.NewGuid().ToString());

    public override HealthResult GetHealth() {
        return Health;
    }
}