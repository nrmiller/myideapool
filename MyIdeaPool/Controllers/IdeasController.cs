using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyIdeaPool.Tools;
using MyIdeaPool.Models;
using MyIdeaPool.Models.Requests;
using MyIdeaPool.Models.Responses;

namespace MyIdeaPool.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IdeasController : ControllerBase
    {
        private readonly IdeaPoolContext dbContext;
        private readonly JwtTokenHelper tokenHelper;

        public IdeasController(IdeaPoolContext dbContext, JwtTokenHelper tokenHelper)
        {
            this.dbContext = dbContext;
            this.tokenHelper = tokenHelper;
        }

        // POST ideas
        [HttpPost]
        public async Task<ActionResult<Idea>> CreateIdea(IdeaRequest request)
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            // Sanitize inputs.
            if (request.Content.Length > 255) return BadRequest("Content cannot exceed 255 characters.");
            if (request.Impact < 1 || request.Impact > 10) return BadRequest("Impact must be between 1 and 10.");
            if (request.Ease < 1 || request.Ease > 10) return BadRequest("Ease must be between 1 and 10.");
            if (request.Confidence < 1 || request.Confidence > 10) return BadRequest("Confidence must be between 1 and 10.");

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);

            var idea = new Idea()
            {
                Id = await GenerateUniqueId(userId),
                UserId = userId,
                Content = request.Content,
                Impact = (int)request.Impact,
                Ease = (int)request.Ease,
                Confidence = (int)request.Confidence,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            dbContext.Ideas.Add(idea);
            await dbContext.SaveChangesAsync();

            var response = new IdeaResponse(idea.Id, idea.Content, idea.Impact, idea.Ease, idea.Confidence, idea.CreatedAt);
            return CreatedAtAction(nameof(GetIdeas), response);
        }

        // DELETE ideas/n
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            var idea = await dbContext.Ideas.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);

            // Users are only allowed to delete their own ideas.
            if (idea.UserId != userId)
            {
                return Unauthorized();
            }

            dbContext.Remove(idea);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        // GET: ideas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Idea>>> GetIdeas([FromQuery]int page)
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            // Sanitize inputs.
            // Note: This check is causing the cm-quiz script to fail the DELETE idea test.
            // The reason it fails is because the cm-quiz is trying to access a page not
            // allowed by the API (the script fails to provide a query string).
            // See post: https://github.com/codementordev/cm-quiz/issues/5#issuecomment-501140778
            if (page <= 0) return BadRequest("Page must be greater or equal to 1.");

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);

            int entriesToSkip = (page - 1) * 10;
            List<Idea> ideas = await dbContext.Ideas
                .Where((Idea it) => it.UserId == userId)
                .OrderByDescending((Idea it) => it.AverageScore)
                .Skip(entriesToSkip)
                .Take(10).ToListAsync();
            return ideas;
        }

        // PUT ideas/n
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIdea(string id, IdeaRequest request)
        {
            // Authenticate requester.
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            // Sanitize inputs.
            if (request.Content.Length > 255) return BadRequest("Content cannot exceed 255 characters.");
            if (request.Impact < 1 || request.Impact > 10) return BadRequest("Impact must be between 1 and 10.");
            if (request.Ease < 1 || request.Ease > 10) return BadRequest("Ease must be between 1 and 10.");
            if (request.Confidence < 1 || request.Confidence > 10) return BadRequest("Confidence must be between 1 and 10.");

            var idea = await dbContext.Ideas.FindAsync(id);

            // Get the user's ID from the claims.
            var userIdString = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;
            int userId = int.Parse(userIdString);

            // Users are only allowed to update their own ideas.
            if (idea.UserId != userId)
            {
                return Unauthorized();
            }

            // Copy request parameters.
            idea.Content = request.Content;
            idea.Impact = (int)request.Impact;
            idea.Ease = (int)request.Ease;
            idea.Confidence = (int)request.Confidence;

            // Attach idea to context and save.
            dbContext.Entry(idea).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            var response = new IdeaResponse(idea.Id, idea.Content, idea.Impact, idea.Ease, idea.Confidence, idea.CreatedAt);
            return Ok(response);
        }

        private async Task<string> GenerateUniqueId(int userId)
        {
            do
            {
                string genId = UrlHelper.GenerateRandomUrl(16);
                bool foundMatch = await dbContext.Ideas
                    .Where((Idea it) => it.UserId == userId)
                    .AnyAsync((Idea it) => it.Id == genId);
                if (!foundMatch)
                {
                    return genId;
                }

            } while (true);
        }
    }
}