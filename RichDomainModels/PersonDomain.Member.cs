using Dtos;
using UnitTestFriendlyDal;

using System.Linq;
using System.Collections.Generic;


namespace RichDomainModels
{
    public static partial class PersonDomain
    {
        public class Member
        {
            public int    MemberId   { get; set; }
            public string MemberName { get; set; }


            public static IEnumerable<PersonDto.Member> GetMembers(IDomainAccess da)
            {
                var list = da.Query<PersonDomain.Member>().MakeCacheable().ToList()
                    .Select(x =>
                        new PersonDto.Member
                        {
                            MemberId = x.MemberId,
                            MemberName = x.MemberName
                        });

                return list;
            }
        }

    }
}
