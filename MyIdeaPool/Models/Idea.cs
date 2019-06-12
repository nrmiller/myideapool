using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MyIdeaPool.Models
{
    [DataContract]
    public class Idea
    {
        [Key]
        [DataMember]
        public string Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        // Reference navigation property to User
        public User User { get; set; }

        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public int Impact { get; set; }
        [DataMember]
        public int Ease { get; set; }
        [DataMember]
        public int Confidence { get; set; }

        [NotMapped]
        [DataMember(Name = "average_score")]
        public double AverageScore
        {
            get
            {
                return (Impact + Ease + Confidence) / 3.0d;
            }
        }

        [DataMember(Name = "created_at")]
        public long CreatedAt { get; set; }
    }
}
