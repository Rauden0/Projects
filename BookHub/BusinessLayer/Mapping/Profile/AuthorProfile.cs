using BusinessLayer.Dto.Author;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class AuthorMappingProfile : AutoMapper.Profile
{
    public AuthorMappingProfile()
    {
        CreateMap<Author, AuthorDto>();
    }
}