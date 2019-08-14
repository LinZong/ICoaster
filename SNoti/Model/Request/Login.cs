using Newtonsoft.Json;

namespace SNotiSSL.Model.Request
{
    public class Login : CommonBody
    {
        [JsonProperty("prefetch_count")]
        public int PrefetchCount { get; set; }

        [JsonProperty("data")]
        public LoginAuthorizationData[] data { get; set; }
    }
}