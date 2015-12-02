using ShaneSpace.GameSite.Domain.Mapping;

namespace ShaneSpace.GameSite.WebApi.ViewModels.User
{
    public class UserViewModel : IMapFrom<Models.User>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}