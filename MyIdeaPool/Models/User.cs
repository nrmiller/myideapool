using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyIdeaPool.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gravatar { get; set; }

        // Colleciton navigation property to Ideas
        public ICollection<Idea> Ideas { get; set; }
    }
}
