using BusinessLayer.Dto.Publisher;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class PublisherMappingProfile : AutoMapper.Profile
{
    public PublisherMappingProfile()
    {
        CreateMap<Publisher, PublisherDto>();
    }
}