using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyIdeaPool.Models.Requests
{
    [DataContract]
    public class RefreshRequest
    {
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
