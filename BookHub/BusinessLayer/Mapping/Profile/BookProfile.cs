using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.Genre;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping.Profile;

public class BookMappingProfile : AutoMapper.Profile
{
    public BookMappingProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(d => d.AuthorsIds,
                opt => opt.MapFrom(s => s.Authors.Select(a => a.Id)))
            .ForMember(d => d.GenresIds,
                opt => opt.MapFrom(s => s.Genres.Select(g => g.Id)))
            .ForMember(d => d.OrderItemIds,
                opt => opt.MapFrom(s => s.OrderItems.Select(o => o.Id)))
            .ForMember(d => d.WishlistItemIds,
                opt => opt.MapFrom(s => s.WishlistItems.Select(w => w.Id)))
            .ForMember(d => d.ReviewsIds,
                opt => opt.MapFrom(s => s.Reviews.Select(r => r.Id)))
            .ForMember(d => d.PrimaryGenre,
                opt => opt.MapFrom(s => s.PrimaryGenre != null
                    ? new GenreDto
                    {
                        Id = s.PrimaryGenre.Id,
                        Name = s.PrimaryGenre.Name
                    }
                    : null)).ForMember(d => d.PublisherName, opt => opt.MapFrom(s => s.Publisher.Name))
            .ForMember(
                d => d.AuthorsNames,
                opt => opt.MapFrom(s => s.Authors.Select(a => a.Name)))
            .ForMember(
                d => d.GenresNames, opt => opt.MapFrom(s => s.Genres.Select(g => g.Name)))
            ;
    }
}