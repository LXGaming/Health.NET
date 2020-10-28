using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LXGaming.Health {

    public class HealthClient : Health {

        private readonly byte[] _buffer = new byte[1];

        public HealthClient(ILogger<Health> logger, EndPoint endPoint) : base(logger, endPoint) {
        }

        public override void Start() {
            Socket.Connect(EndPoint);
        }

        public Task StartAsync() {
            return Socket.ConnectAsync(EndPoint);
        }

        public override void Shutdown() {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }

        public override bool GetStatus() {
            return Socket.Receive(_buffer, SocketFlags.None) == 1 && _buffer[0] == Healthy;
        }

        public async Task<bool> GetStatusAsync() {
            return await Socket.ReceiveAsync(_buffer, SocketFlags.None) == 1 && _buffer[0] == Healthy;
        }
    }
}