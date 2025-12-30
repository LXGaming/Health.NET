using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class Health : IDisposable {

        protected const int MaximumStringSize = 4096; // 4 KiB

        protected readonly ILogger<Health> Logger;
        protected readonly EndPoint EndPoint;
        protected readonly Socket Socket;
        private bool _disposed;

        protected Health(ILogger<Health> logger, EndPoint endPoint) {
            Logger = logger;
            EndPoint = endPoint;
            Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public abstract void Start();

        public abstract void Stop();

        public abstract HealthResult GetHealth();

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            _disposed = true;

            if (disposing) {
                Socket.Dispose();
            }
        }
    }
}