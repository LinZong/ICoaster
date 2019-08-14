using Newtonsoft.Json;

namespace SNotiSSL.Model.Request
{
    public class ControlDevice : CommonBody
    {
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }
        public ControlDeviceData[] data { get; set; }
    }
}