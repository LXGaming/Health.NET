using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace LXGaming.Health.Tests;

public class HealthTest {

    private static readonly IPEndPoint EndPoint = IPEndPoint.Parse("127.0.0.1:4325");
    private static readonly HealthResult[] Healths = [
        new(HealthStatus.Healthy, GenerateString()),
        new(HealthStatus.Unhealthy, GenerateString()),
        new((HealthStatus) 3, GenerateString()),
    ];

    private ILoggerFactory _loggerFactory;
    private Server.Server _server;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
        _loggerFactory = LoggerFactory.Create(builder => {
            // no-op
        });

        _server = new Server.Server(_loggerFactory.CreateLogger<Server.Server>(), EndPoint);
        _server.Start();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _server.Dispose();
        _loggerFactory.Dispose();
    }

    [TestCaseSource(nameof(Healths))]
    public void TestHealth(HealthResult expectedHealth) {
        _server.Health = expectedHealth;

        HealthResult actualHealth;
        var client = new Client.Client(_loggerFactory.CreateLogger<Client.Client>(), EndPoint);
        try {
            client.Start();
            actualHealth = client.GetHealth();
            client.Stop();
        } finally {
            client.Dispose();
        }

        Assert.That(expectedHealth, Is.EqualTo(actualHealth));
    }

    private static string GenerateString() {
        return RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 4096);
    }
}