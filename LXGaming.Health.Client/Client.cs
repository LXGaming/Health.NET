using System.Net;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Client;

public class Client(ILogger<Client> logger, EndPoint endPoint) : HealthClient(logger, endPoint);