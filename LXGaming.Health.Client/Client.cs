using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Client;

public class Client : HealthClient {

    public Client(ILogger<Client> logger, EndPoint endPoint) : base(logger, endPoint) {
    }
}