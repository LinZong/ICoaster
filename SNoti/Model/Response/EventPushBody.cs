using Newtonsoft.Json;

namespace SNotiSSL.Model.Response
{
    public class EventPushBody : CommonBody
    {
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }
        [JsonProperty("delivery_id")]
        public int DeliveryId { get; set; }
        [JsonProperty("event_type")]
        public string EventType { get; set; }
        [JsonProperty("mac")]
        public string Mac { get; set; }
        [JsonProperty("data")]
        public DataPoint data { get; set; }
        [JsonProperty("product_key")]
        public string ProductKey { get; set; }
        [JsonProperty("did")]
        public string Did { get; set; }
        [JsonProperty("group_id")]
        public string GroupId { get; set; }
        [JsonProperty("created_at")]
        public float CreatedAt { get; set; }
    }
}