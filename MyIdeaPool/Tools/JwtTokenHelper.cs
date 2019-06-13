using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MyIdeaPool.Tools
{
    public class JwtTokenHelper
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<JwtTokenHelper> logger;

        public JwtTokenHelper(IConfiguration configuration, ILogger<JwtTokenHelper> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        public bool ValidateJwtToken(string token, out SecurityToken validatedToken)
        {
            if (string.IsNullOrEmpty(token))
            {
                validatedToken = null;
                return false;
            }

            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true, // Validate the server that issued the token.
                ValidateAudience = true, // Validate that the audience is authorized to receive the token.
                ValidateLifetime = true, // Validate that the token is not expired.
                ValidateIssuerSigningKey = true, // Validate the issuer's signing key.
                ValidIssuer = configuration["Jwt:Issuer"], // JWT is issued by the ASP.NET webserver.
                ValidAudience = configuration["Jwt:Issuer"], // ASP.NET webserver processes the JWT on requests.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var user = handler.ValidateToken(token, validationParameters, out validatedToken);
                httpContextAccessor.HttpContext.User = user;
            }
            catch (SecurityTokenException)
            {
                // Validation exceptions are handled.
                validatedToken = null;
                return false;
            }
            catch (Exception)
            {
                // Re-throw exceptions related to bad format.
                throw;
            }

            return true;
        }

        /// <summary>
        /// Creates a long-lived refresh token that can only be used.
        /// The refresh token is URL-safe.
        /// </summary>
        /// <returns>The refresh token.</returns>
        public static string GenerateRefreshToken()
        {
            // Use a CSRNG to get 32 bytes.
            var refreshTokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refreshTokenBytes);

                return refreshTokenBytes.ToHexString();
            }
        }
    }
}
