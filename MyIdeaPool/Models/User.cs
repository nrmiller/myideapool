using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyIdeaPool.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        // Profile-related:
        public string Name { get; set; }
        public string Email { get; set; }
        public string GravatarUrl { get; set; }

        // Security-related:
        public string Salt { get; set; }
        public string SaltedPasswordHash { get; set; }
        public string RefreshToken { get; set; }

        // Colleciton navigation property to Ideas
        public ICollection<Idea> Ideas { get; set; }
    }
}
