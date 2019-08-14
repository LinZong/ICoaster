namespace SNotiSSL.Config
{
    public class SNotiClientConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int PrefetchCount { get; set; }
        public int ReceiveQueueCapacity { get; set; }
        public int ControlQueueCapacity { get; set; }
        public int ReConnectSeconds { get; set; }
        public int HeartbeatIntervalSeconds { get; set; }

        public string ProductKey { get; set; }
        public string AuthId { get; set; }
        public string AuthSecret { get; set; }
        public string SubKey { get; set; }
    }
}