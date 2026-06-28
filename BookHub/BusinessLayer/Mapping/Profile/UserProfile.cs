using BusinessLayer.Dto.User;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class UserMappingProfile : AutoMapper.Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            // convert enum to string for display
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));
    }
}