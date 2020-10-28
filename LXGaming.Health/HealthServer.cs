using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class HealthServer : Health {

        private static readonly byte[] HealthyBuffer = {Healthy};
        private static readonly byte[] UnhealthyBuffer = {Unhealthy};

        protected HealthServer(ILogger<Health> logger, EndPoint endPoint) : base(logger, endPoint) {
        }

        public override void Start() {
            Socket.Bind(EndPoint);
            Socket.Listen(0);
            Socket.BeginAccept(AcceptCallback, Socket);
        }

        public override void Shutdown() {
            Socket.Close();
        }

        private void AcceptCallback(IAsyncResult result) {
            var server = (Socket) result.AsyncState;
            try {
                server.BeginAccept(AcceptCallback, server);
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

            byte[] buffer;
            try {
                buffer = GetStatus() ? HealthyBuffer : UnhealthyBuffer;
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while getting status");
                buffer = UnhealthyBuffer;
            }

            try {
                client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning send to {Client}", client.RemoteEndPoint);
            }
        }

        private void SendCallback(IAsyncResult result) {
            var client = (Socket) result.AsyncState;
            try {
                client.EndSend(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending send to {Client}", client.RemoteEndPoint);
            }

            try {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while closing {Client}", client.RemoteEndPoint);
            }
        }
    }
}