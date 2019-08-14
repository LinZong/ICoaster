using Newtonsoft.Json;

namespace SNotiSSL.Model.Request
{
    public class LoginAuthorizationData
    {
        [JsonProperty("product_key")]
        public string ProductKey { get; set; }
        [JsonProperty("auth_id")]
        public string AuthId { get; set; }
        [JsonProperty("auth_secret")]
        public string AuthSecret { get; set; }
        [JsonProperty("subkey")]
        public string SubKey { get; set; }

        [JsonProperty("events")]
        public string[] Events { get; set; }

        public LoginAuthorizationData()
        {
            // set default events
            Events = new[] {
                "device.attr_fault",
                "device.attr_alert",
                "device.online",
                "device.offline",
                "device.status.raw",
                "device.status.kv",
                "datapoints.changed",
                "center_control.sub_device_added",
                "center_control.sub_device_deleted",
                "device.bind",
                "device.unbind",
                "device.reset",
                "device.file.download",
                "device.app2dev.raw"
            };
        }
    }
}