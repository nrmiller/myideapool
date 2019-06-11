using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyIdeaPool.Models;
using MyIdeaPool.Models.Requests;
using MyIdeaPool.Models.Responses;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
            var userId = HttpContext.User.Claims.First(c => c.Type.Equals("user_id")).Value;

            // TODO We should add to the specific user's ideas.
            var idea = new Idea()
            {
                Id = "af08b7c9d3f",
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

        // DELETE api/ideas/n
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

            // TODO We should delete from the specific user's ideas.
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
            if (page <= 0) return BadRequest("Page must be greater or equal to 1.");

            int entriesToSkip = (page - 1) * 10;

            // TODO Get ideas from specific user.
            return await dbContext.Ideas.Skip(entriesToSkip).Take(10).ToListAsync();
        }

        // PUT ideas/n
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, IdeaRequest request)
        {
            var jwtToken = Request.Headers["X-Access-Token"];
            if (!tokenHelper.ValidateJwtToken(jwtToken, out SecurityToken validatedToken))
            {
                return Unauthorized();
            }

            var idea = await dbContext.Ideas.FindAsync(id);

            // Copy request parameters.
            idea.Content = request.Content;
            idea.Impact = (int)request.Impact;
            idea.Ease = (int)request.Ease;
            idea.Confidence = (int)request.Confidence;

            // TODO Update the idea for the specific user.
            dbContext.Entry(idea).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}