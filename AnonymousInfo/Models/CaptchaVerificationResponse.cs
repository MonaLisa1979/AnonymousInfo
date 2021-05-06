using System.Runtime.Serialization;

namespace AnonymousInfo.Models
{
    [DataContract]
    public class CaptchaVerificationResponse
    {
        [DataMember(Name = "success")]
        public bool Success { get; set; }
        [DataMember(Name = "challenge_ts")]
        public string ChallengeTs { get; set; }
        [DataMember(Name = "hostname")]
        public string Hostname { get; set; }

        [DataMember(Name = "error-codes")]
        public string[] ErrorCodes { get; set; }
    }
}
