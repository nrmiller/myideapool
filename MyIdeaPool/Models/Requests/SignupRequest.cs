using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MyIdeaPool.Models.Requests
{
    [DataContract]
    public class SignupRequest
    {
        [Required]
        [DataMember]
        public string Email { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public string Password { get; set; }
    }
}
