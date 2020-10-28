using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class Health {

        protected const byte Healthy = 0;
        protected const byte Unhealthy = 1;

        protected readonly ILogger<Health> Logger;
        protected readonly EndPoint EndPoint;
        protected readonly Socket Socket;

        protected Health(ILogger<Health> logger, EndPoint endPoint) {
            Logger = logger;
            EndPoint = endPoint;
            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public abstract void Start();

        public abstract void Shutdown();

        public abstract bool GetStatus();
    }
}