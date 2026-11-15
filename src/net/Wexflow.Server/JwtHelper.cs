using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        private static readonly string SecretKey;
        private static readonly byte[] Key;
        private const string Issuer = "wexflow";
        private const string Audience = "wexflow-users";
        public static string AuthCookieName = "wf-auth";

        static JwtHelper()
        {
            var configuredSecret = ConfigurationManager.AppSettings["JwtSecret"];

            if (IsWeakOrDefaultSecret(configuredSecret))
            {
                configuredSecret = LoadOrCreatePersistedSecret();
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

        private static string LoadOrCreatePersistedSecret()
        {
            var fallbackKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jwt.key");

            // 1. Check fallback key file (used if App.config / .exe.config is read-only)
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

            // 3. Persist into Wexflow.Server.exe.config if writable
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings["JwtSecret"] != null)
                {
                    config.AppSettings.Settings["JwtSecret"].Value = newSecret;
                }
                else
                {
                    config.AppSettings.Settings.Add("JwtSecret", newSecret);
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return newSecret;
            }
            catch
            {
                // Fall through to standalone key file if .exe.config cannot be written
            }

            // 4. Fallback: persist to jwt.key next to the server executable
            try
            {
                File.WriteAllText(fallbackKeyFile, newSecret);
            }
            catch
            {
                // Keep the generated key in memory if the filesystem is read-only
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