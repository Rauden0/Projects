using BusinessLayer.Dto.Review;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class ReviewMappingProfile : AutoMapper.Profile
{
    public ReviewMappingProfile()
    {
        CreateMap<Review, ReviewDto>().ForMember(
            dest => dest.UserDisplayName,
            opt => opt.MapFrom(src => src.User.DisplayName)
        );  
    }
}