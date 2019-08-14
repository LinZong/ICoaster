using Newtonsoft.Json;
using SNotiSSL.Model.Response;

namespace SNotiSSL.Model.Request
{
    public class ControlDeviceData : CommonBody
    {
        public ControlDeviceCommand data { get; set; }
    }
    
    public class ControlDeviceCommand
    {
        [JsonProperty("mac")]
        public string Mac { get; set; }

        [JsonProperty("did")]
        public string Did { get; set; }
        [JsonProperty("product_key")]
        public string ProductKey { get; set; }
        public DataPoint attrs { get; set; }
    }
}