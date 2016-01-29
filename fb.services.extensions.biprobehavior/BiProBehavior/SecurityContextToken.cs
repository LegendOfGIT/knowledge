using System.Runtime.Serialization;

namespace FB.Services.Extensions.Behaviors
{
    [DataContract(Name = "Identifier", Namespace = "http://schemas.xmlsoap.org/ws/2005/02/sc")]
    public class SecurityContextToken
    {
        [DataMember(Name = "Identifier")]
        public string Identifier { get; set; }
    }
}
