using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MyIdeaPool.Models.Responses
{
    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "jwt")]
        public string Token { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
