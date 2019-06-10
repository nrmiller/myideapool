using System;
using Microsoft.EntityFrameworkCore;

namespace MyIdeaPool.Models
{
    public class IdeaContext : DbContext
    {
        public IdeaContext(DbContextOptions<IdeaContext> options) : base(options)
        {
        }

        public DbSet<Idea> Ideas { get; set; }
    }
}
