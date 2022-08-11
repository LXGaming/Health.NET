using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class Health : IDisposable {

        public const byte Healthy = 0;
        public const byte Unhealthy = 1;
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

        public abstract (bool, string) GetStatus();

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }

            if (disposing) {
                Socket.Dispose();
            }

            _disposed = true;
        }
    }
}