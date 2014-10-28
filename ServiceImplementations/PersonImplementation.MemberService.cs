using System.Collections.Generic;
using UnitTestFriendlyDal;

namespace ServiceImplementations
{

    public static partial class PersonImplementation
    {
        [System.ServiceModel.ServiceBehavior(InstanceContextMode = System.ServiceModel.InstanceContextMode.PerCall)]
        public class MemberService : ServiceContracts.PersonContract.IMemberService
        {
            IDomainAccessFactory _daf;

            public MemberService(IDomainAccessFactory daf)
            {
                _daf = daf;
            }


            IEnumerable<Dtos.PersonDto.Member> ServiceContracts.PersonContract.IMemberService.GetMembers()
            {
                using (var da = _daf.OpenDomainAccess())
                {
                    return RichDomainModels.PersonDomain.Member.GetMembers(da);
                }
            }

            string ServiceContracts.PersonContract.IMemberService.GetMembership()
            {
                return "Avengers";
            }
        }
    }
}
