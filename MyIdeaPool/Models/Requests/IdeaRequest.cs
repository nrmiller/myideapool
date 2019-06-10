using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MyIdeaPool.Models.Requests
{
    [DataContract]
    public class IdeaRequest
    {
        [Required]
        [DataMember]
        public string Content { get; set; }

        [Required]
        [DataMember]
        public int? Impact { get; set; }

        [Required]
        [DataMember]
        public int? Ease { get; set; }

        [Required]
        [DataMember]
        public int? Confidence { get; set; }
    }
}
