using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyIdeaPool.Models.Requests
{
    [DataContract]
    public class LogoutRequest
    {
        [Required]
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
