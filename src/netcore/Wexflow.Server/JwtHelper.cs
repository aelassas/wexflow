using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Wexflow.Server
{
    public static class JwtHelper
    {
        private const string LegacyDefaultSecret = "b7a3c04f10e84c3f95a3f3497bda8e32";
        private const int MinSecretLength = 32;

        private static string SecretKey = null;
        private static byte[] Key = null;

        public static string Issuer = "wexflow";
        public static string Audience = "wexflow-users";
        public static string AuthCookieName = "wf-auth";

        public static void Initialize(IConfiguration config, string appSettingsPath = null)
        {
            appSettingsPath ??= Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            var configuredSecret = config["JwtSecret"];

            if (IsWeakOrDefaultSecret(configuredSecret))
            {
                configuredSecret = LoadOrCreatePersistedSecret(appSettingsPath);
            }

            SecretKey = configuredSecret;
            Key = Encoding.UTF8.GetBytes(SecretKey);
        }

        private static bool IsWeakOrDefaultSecret(string secret)
        {
            return string.IsNullOrWhiteSpace(secret)
                || secret.Length < MinSecretLength
                || string.Equals(secret, LegacyDefaultSecret, StringComparison.Ordinal);
        }

        private static string LoadOrCreatePersistedSecret(string appSettingsPath)
        {
            var fallbackKeyFile = Path.Combine(AppContext.BaseDirectory, "jwt.key");

            // 1. Check fallback key file (used if appsettings.json is read-only)
            if (File.Exists(fallbackKeyFile))
            {
                var existing = File.ReadAllText(fallbackKeyFile).Trim();
                if (!IsWeakOrDefaultSecret(existing))
                {
                    return existing;
                }
            }

            // 2. Generate a cryptographically random 256-bit (32-byte) key as a 64-char hex string
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var newSecret = BitConverter.ToString(randomBytes).Replace("-", string.Empty).ToLowerInvariant();

            // 3. Persist into appsettings.json if present and writable
            try
            {
                if (File.Exists(appSettingsPath))
                {
                    var json = File.ReadAllText(appSettingsPath);
                    var root = JObject.Parse(json, new JsonLoadSettings
                    {
                        CommentHandling = CommentHandling.Ignore
                    });
                    root["JwtSecret"] = newSecret;
                    File.WriteAllText(appSettingsPath, root.ToString(Formatting.Indented));
                    return newSecret;
                }
            }
            catch
            {
                // Fall through to standalone key file if appsettings.json cannot be written
            }

            // 4. Fallback: persist to jwt.key next to the server binary
            try
            {
                File.WriteAllText(fallbackKeyFile, newSecret);
            }
            catch
            {
                // If the entire filesystem is read-only, keep the generated key in memory for this process lifetime
            }

            return newSecret;
        }

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

            var expires = stayConnected
                ? now.AddYears(1)
                : now.AddMinutes(expireMinutes);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: signingCredentials
            );

            return tokenHandler.WriteToken(token);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ValidIssuer = Issuer,
                ValidAudience = Audience
            };

            try
            {
                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}