using ShaneSpace.GameSite.Models;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    public static class HubExtensions
    {
        public static string SingalRIdentity(this User identity)
        {
            return identity.Email;
        }
    }
}