using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyIdeaPool.Models;
using MyIdeaPool.Models.Requests;
using MyIdeaPool.Models.Responses;

namespace MyIdeaPool.Controllers
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IdeaPoolContext dbContext;
        private readonly JwtTokenHelper tokenHelper;
        private readonly ILogger<UsersController> logger;

        public UsersController(IConfiguration configuration, IdeaPoolContext dbContext, JwtTokenHelper tokenHelper, ILogger<UsersController> logger)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
            this.tokenHelper = tokenHelper;
            this.logger = logger;
        }

        // POST users
        [HttpPost("users")]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
            // For a production app, it would make sense to validate
            // the format for the e-mail. E-mail technically has many 
            // rules, and since we don't need to send any e-mail, we won't
            // restrict the input.

            (bool passwordValid, string error) = ValidatePassword(request.Password);
            if (!passwordValid)
            {
                return BadRequest(error);
            }

            // Check if account already exists.
            bool accountExists = dbContext.Users.Any(user => user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
            if (accountExists)
            {
                return BadRequest("Cannot create user! Account already exists.");
            }

            byte[] salt = SecurityHelper.GenerateSalt();
            byte[] saltedHash = SecurityHelper.GenerateSaltedHash(Encoding.UTF8.GetBytes(request.Password), salt);

            // Create new user:
            User newUser = new User
            {
                Email = request.Email,
                Name = request.Name,
                Salt = salt.ToHexString(),
                SaltedPasswordHash = saltedHash.ToHexString()
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            TokenResponse token = new TokenResponse()
            {
                Token = CreateTokenFor(newUser),
                RefreshToken = JwtTokenHelper.GenerateRefreshToken()
            };

            // Remember to persist the refresh token:
            newUser.RefreshToken = token.RefreshToken;
            await dbContext.SaveChangesAsync();

            return Ok(token);
        }

        private (bool success, string error) ValidatePassword(string password)
        {
            if (password.Length < 8)
                return (false, "Password must have at least 8 characters.");
            if (!Regex.Match(password, @"[a-z]").Success)
                return (false, "Password doesn't must have at least 1 lowercase letter.");
            if (!Regex.Match(password, @"[A-Z]").Success)
                return (false, "Password doesn't must have at least 1 uppercase letter.");
            if (!Regex.Match(password, @"\d+").Success)
                return (false, "Password doesn't must have at least 1 number.");

            return (true, string.Empty);
        }

        // POST access-tokens
        [HttpPost("access-tokens")]
        public async Task<ActionResult<TokenResponse>> Login(LoginRequest credentials)
        {
            // Check if account already exists.
            User user = dbContext.Users.FirstOrDefault(it => it.Email.Equals(credentials.Email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return Unauthorized();
            }

            bool isAuthenticated = Authenticate(credentials, user);
            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            TokenResponse token = new TokenResponse()
            {
                Token = CreateTokenFor(user),
                RefreshToken = JwtTokenHelper.GenerateRefreshToken()
            };

            user.RefreshToken = token.RefreshToken;
            await dbContext.SaveChangesAsync();

            return token;
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

        private bool Authenticate(LoginRequest credentials, User user)
        {
            return SecurityHelper.VerifyPassword(
                password: Encoding.UTF8.GetBytes(credentials.Password),
                salt: SecurityHelper.ToBytes(user.Salt),
                saltedHash: SecurityHelper.ToBytes(user.SaltedPasswordHash));
        }

        private string CreateTokenFor(User user)
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
    }
}
