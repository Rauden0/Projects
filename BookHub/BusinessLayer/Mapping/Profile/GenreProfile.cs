using BusinessLayer.Dto.Genre;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class GenreMappingProfile : AutoMapper.Profile
{
    public GenreMappingProfile()
    {
        CreateMap<Genre, GenreDto>();
    }
}