using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyIdeaPool.Models.Responses
{
    [DataContract]
    public class RefreshResponse
    {
        [DataMember(Name = "jwt")]
        public string Token { get; set; }
    }
}
