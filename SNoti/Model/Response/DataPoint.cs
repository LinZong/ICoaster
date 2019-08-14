using Newtonsoft.Json;

namespace SNotiSSL.Model.Response
{
    public class DataPoint
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Buzzer { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Water { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Temperature { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Hour { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? humidity { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Remind_drink_switch { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Recommended_amount_of_water { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Minute { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Year { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Month { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Day { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Total_amount_of_water { get; set; }
    }
}