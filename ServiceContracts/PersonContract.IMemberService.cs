using Dtos;
using System.Collections.Generic;
using System.ServiceModel;

namespace ServiceContracts
{
    public static partial class PersonContract
    {

        [ServiceContract]
        public interface IMemberService
        {
            [OperationContract]
            string GetMembership();


            [OperationContract]
            IEnumerable<PersonDto.Member> GetMembers();
        }
    }
}
