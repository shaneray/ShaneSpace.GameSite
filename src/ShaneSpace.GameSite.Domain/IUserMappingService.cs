using System.Security.Principal;
using ShaneSpace.GameSite.Models;

namespace ShaneSpace.GameSite.Domain
{
    public interface IUserMappingService
    {
        User GetUserFromIdentity(IIdentity user);
    }
}