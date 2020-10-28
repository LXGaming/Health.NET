using CommandLine;

namespace LXGaming.Health.Server {

    public class Options {

        [Option('a', "address", Default = "127.0.0.1")]
        public string Address { get; set; }

        [Option('p', "port", Default = 4325)]
        public int Port { get; set; }
    }
}