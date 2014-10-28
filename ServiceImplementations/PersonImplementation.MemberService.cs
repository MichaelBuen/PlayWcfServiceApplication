using System.Collections.Generic;

namespace ServiceImplementations
{
    public static partial class PersonImplementation
    {
        [System.ServiceModel.ServiceBehavior(InstanceContextMode = System.ServiceModel.InstanceContextMode.PerCall)]
        public class MemberService : ServiceContracts.PersonContract.IMemberService
        {

            IEnumerable<Dtos.PersonDto.Member> ServiceContracts.PersonContract.IMemberService.GetMembers()
            {
                yield return new Dtos.PersonDto.Member { MemberId = 1, MemberName = "John" };
                yield return new Dtos.PersonDto.Member { MemberId = 2, MemberName = "Paul" };
                yield return new Dtos.PersonDto.Member { MemberId = 3, MemberName = "George" };
                yield return new Dtos.PersonDto.Member { MemberId = 4, MemberName = "Ringo" };
            }

            string ServiceContracts.PersonContract.IMemberService.GetMembership()
            {
                return "Avengers";
            }
        }
    }
}
