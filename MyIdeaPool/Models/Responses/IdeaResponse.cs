using System;
using System.Runtime.Serialization;

namespace MyIdeaPool.Models.Responses
{
    [DataContract]
    public class IdeaResponse
    {
        public IdeaResponse(string id, string content, int impact, int ease, int confidence, long createdAt)
        {
            Id = id;
            Content = content;
            Impact = impact;
            Ease = ease;
            Confidence = confidence;
            CreatedAt = createdAt;
        }

        [DataMember]
        public string Id { get; }
        [DataMember]
        public string Content { get; }
        [DataMember]
        public int Impact { get; }
        [DataMember]
        public int Ease { get; }
        [DataMember]
        public int Confidence { get; }

        [DataMember(Name = "average_score")]
        public double AverageScorce
        {
            get
            {
                return (Impact + Ease + Confidence) / 3.0d;
            }
        }

        [DataMember(Name = "created_at")]
        public long CreatedAt { get; }
    }
}
