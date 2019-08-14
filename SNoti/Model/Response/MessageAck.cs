using Newtonsoft.Json;

namespace SNotiSSL.Model.Response
{
    public class MessageAck : CommonBody
    {

        [JsonProperty("delivery_id",NullValueHandling = NullValueHandling.Ignore)]
        public int DeliveryId { get; set; }

        [JsonProperty("msg_id",NullValueHandling = NullValueHandling.Ignore)]
        public string MsgId { get; set; }
        public MessageAck()
        {
            cmd = SNotiCommandType.Event_ACK;
        }
    }
}