using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using ICoaster.Model.DependencyInjection.Token;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ICoaster.Services.Token
{
    public class TokenManager
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _accessor;
        private readonly JwtTokenConfig _config;

        public TokenManager(IDistributedCache cache, IHttpContextAccessor accessor, IOptions<JwtTokenConfig> options)
        {
            _cache = cache;
            _accessor = accessor;
            _config = options.Value;
        }

        public async Task<bool> IsCurrentTokenActive() => await IsTokenActive(GetCurrentToken());

        public async Task RevokeCurrentToken() => await RevokeToken(GetCurrentToken());

        public async Task<bool> IsTokenActive(string token) => !string.IsNullOrEmpty(token) && await _cache.GetStringAsync(GetCacheKey(token)) == null;

        public async Task RevokeToken(string token) => await _cache.SetStringAsync(GetCacheKey(token),"revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_config.ExpireSpan)
            });

        public string GetCacheKey(string token) => $"token:{token}:revoked";

        public string GetCurrentToken()
        {
            var JwtBearer = _accessor.HttpContext.Request.Headers["Authorization"];
            return StringValues.Empty == JwtBearer ? string.Empty : JwtBearer.Single().Split(" ").Last();
        }
    }
}
