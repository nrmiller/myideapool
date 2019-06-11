using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyIdeaPool.Models
{
    public class Credentials
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        // Reference navigation property to User
        public User User { get; set; }

        public string Salt { get; set; }
        public string PasswordHash { get; set; }
    }
}
