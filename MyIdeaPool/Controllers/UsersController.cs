using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            bool accountExists = await dbContext.Users.AnyAsync(user => user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
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
                AvatarUrl = GenerateGravatarUrl(request.Email),
                Salt = salt.ToHexString(),
                SaltedPasswordHash = saltedHash.ToHexString()
            };

            // Save the user so we can retrieve the ID.
            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();


            TokenResponse response = new TokenResponse()
            {
                Token = CreateTokenFor(newUser),
                RefreshToken = await GenerateUniqueRefreshToken()
            };

            // Remember to persist the refresh token:
            newUser.RefreshToken = response.RefreshToken;
            await dbContext.SaveChangesAsync();

            return Ok(response);
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
            User user = await dbContext.Users.FirstOrDefaultAsync(it => it.Email.Equals(credentials.Email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return Unauthorized();
            }

            bool isAuthenticated = Authenticate(credentials, user);
            if (!isAuthenticated)
            {
                return Unauthorized();
            }
            
            TokenResponse response = new TokenResponse()
            {
                Token = CreateTokenFor(user),
                RefreshToken = await GenerateUniqueRefreshToken()
            };
            
            user.RefreshToken = response.RefreshToken;
            await dbContext.SaveChangesAsync();

            return response;
        }

        // DELETE access-tokens
        [HttpDelete("access-tokens")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);
            User user = await dbContext.Users.FindAsync(userId);

            if (!user.RefreshToken.Equals(request.RefreshToken))
            {
                // The JWT bearer had a valid JWT; however, they did not
                // have the appropriate refresh token.

                // Important Security Note:
                // The server shall not respond conditionally upon the request.
                // Otherwise, a malicious client can determine which refresh
                // tokens are not valid, narrowing the space of valid tokens
                // to search from.
                // Since in this case the client must also be a JWT bearer,
                // they have a relatively short window to eliminate posible
                // refresh tokens.
                return NoContent();
            }

            // Delete the refresh token.
            user.RefreshToken = null;

            await dbContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);
            User user = await dbContext.Users.FindAsync(userId);

            UserInfoResponse response = new UserInfoResponse()
            {
                Email = user.Email,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl
            };

            return Ok(response);
        }


        // POST access-tokens/refresh
        [HttpPost("access-tokens/refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            User user = await dbContext.Users.FirstOrDefaultAsync(it => it.RefreshToken.Equals(request.RefreshToken, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                // The refresh token present by the client is invalid.
                return Unauthorized();
            }

            // The refresh token was accepted.
            // Security Note: We don't need to regenerate the refresh token
            // since the owner has already been authenticated.
            // An impersonating JWT bearer will only have access util token
            // expiry.
            // The JWT bearer will only have the refresh token under
            // the scenario that they have the account credentials.

            RefreshResponse response = new RefreshResponse()
            {
                Token = CreateTokenFor(user)
            };

            return Ok(response);
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
                    new Claim("user_id", user.Id.ToString()),
                },
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateUniqueRefreshToken()
        {
            do
            {
                string refreshToken = JwtTokenHelper.GenerateRefreshToken();
                bool foundMatch = await dbContext.Users.AnyAsync((User it) => it.RefreshToken != null && it.RefreshToken.Equals(refreshToken));
                if (!foundMatch)
                {
                    return refreshToken;
                }

            } while (true);
        }

        private string GenerateGravatarUrl(string email)
        {
            using (var md5 = MD5.Create())
            {
                string sanitizedEmail = email.Trim().ToLower();
                byte[] emailBytes = Encoding.UTF8.GetBytes(sanitizedEmail);
                byte[] hash = md5.ComputeHash(emailBytes);
                return $"https://www.gravatar.com/avatar/{hash.ToHexString()}";
            }
        }
    }
}
