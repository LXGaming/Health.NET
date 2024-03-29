﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LXGaming.Health.Models;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public abstract class HealthClient : Health {

        private readonly byte[] _buffer = new byte[1 + MaximumStringSize];
        private readonly ManualResetEventSlim _state = new ManualResetEventSlim(false);
        private int _length;
        private bool _disposed;

        protected HealthClient(ILogger<HealthClient> logger, EndPoint endPoint) : base(logger, endPoint) {
        }

        public override void Start() {
            _state.Reset();
            Socket.BeginConnect(EndPoint, ConnectCallback, Socket);
        }

        public override void Stop() {
            if (Socket.Connected) {
                Socket.Shutdown(SocketShutdown.Both);
            }

            Socket.Close();
        }

        public override Status GetStatus() {
            _state.Wait();
            switch (_length) {
                case 0:
                    return new Status(false, null);
                case 1:
                    return new Status(_buffer[0] == Healthy, null);
                default:
                    return new Status(_buffer[0] == Healthy, Encoding.UTF8.GetString(_buffer, 1, _length - 1));
            }
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
                client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning receive from {Server}", (client.RemoteEndPoint ?? EndPoint).ToString());
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
                Logger.LogError(ex, "Encountered an error while ending receive from {Server}", (client.RemoteEndPoint ?? EndPoint).ToString());
            }

            try {
                client.BeginDisconnect(true, DisconnectCallback, client);
            } catch (Exception ex) {
                Logger.LogError(ex, "Encountered an error while beginning disconnect from {Server}", (client.RemoteEndPoint ?? EndPoint).ToString());
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
                Logger.LogError(ex, "Encountered an error while ending disconnect from {Server}", (client.RemoteEndPoint ?? EndPoint).ToString());
            } finally {
                _state.Set();
            }
        }

        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    _state.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}