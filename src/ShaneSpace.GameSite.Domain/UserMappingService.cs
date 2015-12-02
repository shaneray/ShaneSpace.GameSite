using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace ShaneSpace.GameSite.Domain
{
    public class UserMappingService : IUserMappingService
    {
        private readonly CoreContext _context;
        private static ConcurrentDictionary<int, User> userCache { get; set; }

        public UserMappingService(CoreContext context)
        {
            userCache = new ConcurrentDictionary<int, User>();
            _context = context;
        }

        public User GetUserFromIdentity(IIdentity user)
        {
            var claimsUser = (ClaimsIdentity)user;
            var output = new User();
            foreach (Claim claim in claimsUser.Claims)
            {
                switch (claim.Type)
                {
                    case ShaneSpaceClaimTypes.Id:
                        output.Id = int.Parse(claim.Value);
                        break;
                    case ShaneSpaceClaimTypes.DisplayName:
                        output.DisplayName = claim.Value;
                        break;
                    case ShaneSpaceClaimTypes.Email:
                        output.Email = claim.Value;
                        break;
                }
            }
            
            var dbUser = userCache.GetOrAdd(output.Id, _context.Users.AsNoTracking().Where(x => x.AuthId == output.Id).Single());
            output.Id = dbUser.Id;

            return output;
        }
    }
}