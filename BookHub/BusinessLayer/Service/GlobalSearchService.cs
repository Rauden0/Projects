using BusinessLayer.Dto.GlobalSearch;

namespace BusinessLayer.Service;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IBookService _bookService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;

    public GlobalSearchService(
        IBookService bookService,
        IGenreService genreService,
        IPublisherService publisherService)
    {
        _bookService = bookService;
        _genreService = genreService;
        _publisherService = publisherService;
    }

    public async Task<GlobalSearchDto> SearchAll(string term)
    {
        var books = await _bookService.SearchBooks(term);
        var genre = await _genreService.SearchGenres(term);
        var publishers = await _publisherService.SearchPublishers(term);

        return new GlobalSearchDto
        {
            Books = books.IfFail([]),
            Genres = genre.IfFail([]),
            Publishers = publishers.IfFail([])
        };
    }
}