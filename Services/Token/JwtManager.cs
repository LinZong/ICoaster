using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ICoaster.Model.DependencyInjection.Token;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ICoaster.Services.Token
{
    public class JwtManager
    {
        private readonly JwtTokenConfig _options;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly SecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;

        private readonly ILogger<JwtManager> _logger;

        public JwtManager(IOptions<JwtTokenConfig> options, ILogger<JwtManager> logger)
        {
            _options = options.Value;
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey));
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
            _logger = logger;
        }

        public JwtResponse Create(string user)
        {
            var expires = DateTime.Now.AddSeconds(_options.ExpireSpan);
            var jwt = new JwtSecurityToken(_options.Issuer, _options.Audience,
                new[]
                {
                    new Claim(ClaimTypes.Name,user)
                },
                expires: expires,
                signingCredentials: _signingCredentials
            );
            var token = _jwtSecurityTokenHandler.WriteToken(jwt);
            return new JwtResponse
            {
                AccessToken = token,
                ExpireTime = expires.ToString("yyyy/M/d HH:mm:ss")
            };
        }

        public JwtSecurityToken ValidateJwtToken(string token, TokenValidationParameters validationParameters)
        {
            try
            {
                _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                _logger.LogInformation($"Token {token} 验证成功!");
                return ((JwtSecurityToken)securityToken);
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                _logger.LogError($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                 _logger.LogError($"Token was invalid: {argex.Message}");
            }
            return null;
        }
    }
}
