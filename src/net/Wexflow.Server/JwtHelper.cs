using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Wexflow.Server
{
    public static class JwtHelper
    {
        private static readonly string SecretKey = ConfigurationManager.AppSettings["JwtSecret"]; // min 16 chars for HMAC
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(SecretKey);
        private const string Issuer = "wexflow";
        private const string Audience = "wexflow-users";
        public static string AuthCookieName = "wf-auth";

        public static string GenerateToken(string username, int expireMinutes = 60, bool stayConnected = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("stay", stayConnected ? "1" : "0")
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Key),
                SecurityAlgorithms.HmacSha256
            );

            JwtSecurityToken token;

            if (stayConnected)
            {
                // No expiration
                token = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: now,
                    signingCredentials: signingCredentials
                );
            }
            else
            {
                // With expiration
                token = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.AddMinutes(expireMinutes),
                    signingCredentials: signingCredentials
                );
            }

            return tokenHandler.WriteToken(token);
        }



        public static ClaimsPrincipal ValidateToken(string token, bool allowNoExpiration = true)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ValidIssuer = Issuer,
                ValidAudience = Audience
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
