using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health.Server {

    public static class Program {

        private static readonly ILogger<HealthServer> Logger = LoggerFactory.Create(builder => {
            builder.AddConsole();
        }).CreateLogger<HealthServer>();

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

            HealthServer server;
            try {
                server = new Server(Logger, endPoint);
                server.Start();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while starting server");
                return 1;
            }

            Logger.LogInformation("Listening on {Endpoint}", endPoint);

            try {
                Logger.LogInformation("Press Enter to continue...");
                Console.ReadLine();
                return 0;
            } catch (Exception) {
                return 1;
            } finally {
                try {
                    server.Shutdown();
                } catch (Exception ex) {
                    Logger.LogError(ex, "Encountered an error while shutting down server");
                }
            }
        }

        private static int Failure(IEnumerable<Error> errors) {
            return 1;
        }
    }
}