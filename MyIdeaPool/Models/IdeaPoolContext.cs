using System;
using Microsoft.EntityFrameworkCore;

namespace MyIdeaPool.Models
{
    public class IdeaPoolContext : DbContext
    {
        public IdeaPoolContext(DbContextOptions<IdeaPoolContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Idea> Ideas { get; set; }
    }
}
