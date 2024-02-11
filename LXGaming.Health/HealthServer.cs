using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LXGaming.Health.Models;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class HealthServer : Health {

        private bool _closed;
        private bool _disposed;

        protected HealthServer(ILogger<HealthServer> logger, EndPoint endPoint) : base(logger, endPoint) {
        }

        public override void Start() {
            Socket.Bind(EndPoint);
            Socket.Listen(0);
            Socket.BeginAccept(AcceptCallback, Socket);
        }

        public override void Stop() {
            Socket.Close();
            _closed = true;
        }

        private void AcceptCallback(IAsyncResult result) {
            if (_closed || _disposed) {
                return;
            }

            var server = (Socket) result.AsyncState;
            if (server == null) {
                Logger.LogError("Server is unavailable");
                return;
            }

            try {
                server.BeginAccept(AcceptCallback, server);
            } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted) {
                return;
            } catch (ObjectDisposedException) {
                return;
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning accept");
                return;
            }

            Socket client;
            try {
                client = server.EndAccept(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending accept");
                return;
            }

            Status status;
            try {
                status = GetStatus();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while getting status");
                status = new Status(false, null);
            }

            CalculateEncodingLength(Encoding.UTF8, status.Message, MaximumStringSize, out var characterCount, out var messageSize);

            var buffer = new byte[1 + messageSize];
            buffer[0] = status.State ? Healthy : Unhealthy;
            if (status.Message != null) {
                Encoding.UTF8.GetBytes(status.Message, 0, characterCount, buffer, 1);
            }

            try {
                client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning send to {Client}", client.RemoteEndPoint?.ToString());
            }
        }

        private void SendCallback(IAsyncResult result) {
            var client = (Socket) result.AsyncState;
            if (client == null) {
                Logger.LogError("Client is unavailable");
                return;
            }

            try {
                client.EndSend(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending send to {Client}", client.RemoteEndPoint?.ToString());
            }

            try {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while closing {Client}", client.RemoteEndPoint?.ToString());
            }
        }

        private static void CalculateEncodingLength(Encoding encoding, string @string, int maximumSize, out int index, out int size) {
            size = 0;
            for (index = 0; index < @string?.Length; index++) {
                var byteCount = encoding.GetByteCount(@string, index, 1);
                if (size + byteCount > maximumSize) {
                    return;
                }

                size += byteCount;
            }
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // no-op
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}