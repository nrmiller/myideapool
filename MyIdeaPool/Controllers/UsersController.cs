using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyIdeaPool.Models;
using MyIdeaPool.Models.Requests;
using MyIdeaPool.Models.Responses;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyIdeaPool.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly JwtTokenHelper tokenHelper;
        private readonly ILogger<UsersController> logger;

        public UsersController(IConfiguration configuration, JwtTokenHelper tokenHelper, ILogger<UsersController> logger)
        {
            this.configuration = configuration;
            this.tokenHelper = tokenHelper;
            this.logger = logger;
        }

        // POST users
        [HttpPost("users")]
        public IActionResult Signup(SignupRequest request)
        {
            return Ok();
        }

        // POST access-tokens
        [HttpPost("access-tokens")]
        public ActionResult<TokenResponse> Login(LoginRequest credentials)
        {
            User user = Authenticate(credentials);
            if (user != null)
            {
                return new TokenResponse()
                {
                    Token = CreateToken(user),
                    RefreshToken = CreateRefreshToken()
                };
            }
            else
            {
                return Unauthorized();
            }
        }

        // DELETE access-tokens
        [HttpDelete("access-tokens")]
        public IActionResult Logout(TokenResponse refreshToken)
        {
            return Ok();
        }

        // POST access-tokens/refresh
        [HttpPost("access-tokens/refresh")]
        public IActionResult Refresh(TokenResponse refreshToken)
        {
            return Ok();
        }

        private User Authenticate(LoginRequest credentials)
        {
            return new User()
            {
                Id = 1130488,
                Email = credentials.Email
            };
        }

        private string CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Issuer"],
                claims: new List<Claim>()
                {
                    // Add the user's ID as a claim.
                    new Claim("user_id", user.Id.ToString())
                },
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Creates a long-lived refresh token that can only be used.
        /// The refresh token is URL-safe.
        /// </summary>
        /// <returns>The refresh token.</returns>
        private string CreateRefreshToken()
        {
            // Use a CSRNG to get 32 bytes.
            var refreshTokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refreshTokenBytes);


                return BitConverter.ToString(refreshTokenBytes).ToLower().Replace("-", string.Empty);
                //return Base64UrlEncoder.Encode(refreshTokenBytes);
            }
        }
    }
}
