namespace ICoaster.Model.DependencyInjection.Token
{
    public class JwtTokenConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecurityKey { get; set; }
        public int ExpireSpan { get; set; }
    }

    public class JwtResponse
    {
        public string AccessToken { get; set; }
        public string ExpireTime { get; set; }
    }
}
