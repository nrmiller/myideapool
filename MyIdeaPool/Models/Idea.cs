using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyIdeaPool.Models
{
    public class Idea
    {
        [Key]
        public string Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        // Reference navigation property to User
        public User User { get; set; }

        public string Content { get; set; }
        public int Impact { get; set; }
        public int Ease { get; set; }
        public int Confidence { get; set; }

        [NotMapped]
        public double AverageScore
        {
            get
            {
                return (Impact + Ease + Confidence) / 3.0d;
            }
        }

        public long CreatedAt { get; set; }
    }
}
