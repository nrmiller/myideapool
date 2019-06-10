using System;
using System.ComponentModel.DataAnnotations;

namespace MyIdeaPool.Models
{
    public class Idea
    {
        public string Id { get; set; } // Required by DbContext

        public string Content { get; set; }

        public int Impact { get; set; }

        public int Ease { get; set; }

        public int Confidence { get; set; }

        public long CreatedAt { get; set; }
    }
}
