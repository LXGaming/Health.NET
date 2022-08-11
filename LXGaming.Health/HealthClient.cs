﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class HealthClient : Health {

        private readonly byte[] _buffer = new byte[1];
        private readonly ManualResetEvent _event = new ManualResetEvent(false);
        private int _length;
        private bool _disposed;

        protected HealthClient(ILogger<HealthClient> logger, EndPoint endPoint) : base(logger, endPoint) {
        }

        public override void Start() {
            _event.Reset();
            Socket.BeginConnect(EndPoint, ConnectCallback, Socket);
        }

        public override void Stop() {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        public override bool GetStatus() {
            _event.WaitOne();
            return _length == 1 && _buffer[0] == Healthy;
        }

        private void ConnectCallback(IAsyncResult result) {
            var client = (Socket) result.AsyncState;
            if (client == null) {
                Logger.LogError("Client is unavailable");
                return;
            }

            try {
                client.EndConnect(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending connect");
                return;
            }

            try {
                client.BeginReceive(_buffer, 0, 1, SocketFlags.None, ReceiveCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning receive from {Server}", client.RemoteEndPoint.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            var client = (Socket) result.AsyncState;
            if (client == null) {
                Logger.LogError("Client is unavailable");
                return;
            }

            try {
                _length = client.EndReceive(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending receive from {Server}", client.RemoteEndPoint.ToString());
            }

            try {
                client.BeginDisconnect(true, DisconnectCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning disconnect from {Server}", client.RemoteEndPoint.ToString());
            }
        }

        private void DisconnectCallback(IAsyncResult result) {
            var client = (Socket) result.AsyncState;
            if (client == null) {
                Logger.LogError("Client is unavailable");
                return;
            }

            try {
                client.EndDisconnect(result);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while ending disconnect from {Server}", client.RemoteEndPoint.ToString());
            } finally {
                _event.Set();
            }
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _event.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}