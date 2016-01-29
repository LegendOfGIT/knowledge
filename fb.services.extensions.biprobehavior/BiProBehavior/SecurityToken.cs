using System.Runtime.Serialization;

namespace FB.Services.Extensions.Behaviors
{
    [DataContract(Name = "SecurityContextToken", Namespace = "http://schemas.xmlsoap.org/ws/2005/02/sc")]
    public class SecurityToken
    {
        [DataMember(Name = "SecurityContextToken")]
        public SecurityContextToken SecurityContextToken { get; set; }
    }
}
