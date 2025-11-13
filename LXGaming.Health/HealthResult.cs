namespace LXGaming.Health {

    public readonly struct HealthResult {

        public HealthStatus Status { get; }

        public string? Message { get; }

        public HealthResult(HealthStatus status, string? message = null) {
            Status = status;
            Message = message;
        }

        public HealthResult(bool status, string? message = null)
            : this(status ? HealthStatus.Healthy : HealthStatus.Unhealthy, message) {
            // no-op
        }

        public override string ToString() {
            return $"[{Status}] {Message}";
        }
    }
}