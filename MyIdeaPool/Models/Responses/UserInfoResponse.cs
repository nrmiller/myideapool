using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyIdeaPool.Models.Responses
{
    [DataContract]
    public class UserInfoResponse
    {
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }
    }
}
