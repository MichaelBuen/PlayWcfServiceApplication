using System.Runtime.Serialization;

namespace Dtos
{
    public static partial class PersonDto
    {
        [DataContract]
        public class Member
        {
            [DataMember]
            public int    MemberId   { get; set; }
            [DataMember]
            public string MemberName { get; set; }
        }
    }
}
