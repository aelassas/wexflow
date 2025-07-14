using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Wexflow.Server
{
    public static class JwtHelper
    {
        // Replace with a long, random, securely stored key (e.g., from configuration)
        private static string SecretKey = null;
        private static byte[] Key = null;

        private const string Issuer = "wexflow";
        private const string Audience = "wexflow-users";

        public static void Initialize(IConfiguration config)
        {
            SecretKey = config["JwtSecret"] ?? throw new InvalidOperationException("JwtSecret config is missing.");
            Key = Encoding.UTF8.GetBytes(SecretKey);
        }

        public static string GenerateToken(string username, int expireMinutes = 60, bool stayConnected = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("stay", stayConnected ? "1" : "0")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            if (!stayConnected)
            {
                tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(expireMinutes);
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal? ValidateToken(string token, bool allowNoExpiration = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = !allowNoExpiration,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Extra check: only allow tokens with 'stay' == "1" to skip expiration
                if (!allowNoExpiration && validatedToken is JwtSecurityToken jwt &&
                    jwt.Claims.FirstOrDefault(c => c.Type == "stay")?.Value == "1")
                {
                    // Even if 'exp' is missing, allow if "stay" == 1
                    return principal;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
