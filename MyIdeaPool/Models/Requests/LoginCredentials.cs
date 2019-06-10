using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MyIdeaPool.Models.Requests
{
    [DataContract]
    public class LoginCredentials
    {
        [Required]
        [DataMember]
        public string Email { get; set; }

        [Required]
        [DataMember]
        public string Password { get; set; }
    }
}
