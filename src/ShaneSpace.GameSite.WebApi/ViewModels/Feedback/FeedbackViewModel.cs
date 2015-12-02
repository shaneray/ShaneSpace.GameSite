using AutoMapper;
using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels.User;

namespace ShaneSpace.GameSite.WebApi.ViewModels.Feedback
{
    public class FeedbackViewModel : ICustomMapping<Models.Feedback, FeedbackViewModel>
    {
        public int FeedbackId { get; set; }
        public UserSummaryViewModel User { get; set; }
        public FeedbackTypes FeedbackType { get; set; }
        public string FeedbackText { get; set; }

        public IMappingExpression<Models.Feedback, FeedbackViewModel> CreateMappings(IConfiguration configuration)
        {
            return Mapper.CreateMap<Models.Feedback, FeedbackViewModel>()
                .ForMember(dest => dest.FeedbackType, src => src.MapFrom(x => (FeedbackTypes)x.FeedbackType));
        }
    }
}