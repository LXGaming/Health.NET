namespace LXGaming.Health.Models {

    public readonly struct Status {

        public bool State { get; }

        public string Message { get; }

        public Status(bool state, string message) {
            State = state;
            Message = message;
        }

        public override string ToString() {
            return $"[{(State ? "Healthy" : "Unhealthy")}] {Message}";
        }
    }
}