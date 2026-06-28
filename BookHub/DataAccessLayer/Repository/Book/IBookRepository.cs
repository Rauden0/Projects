     namespace DataAccessLayer.Repository.Book;

    public interface IBookRepository : IRepository<Models.Book>
    {
        Task<Models.Book?> GetBookByIdwithAuthorAndGenre(int id);

        //TODO will be in Author repository
        Task<List<Models.Author>> GetAuthorsByListIds(List<int> ids);

        //TODO will be in Genre rep
        Task<List<Models.Genre>> GetGenresByListIds(List<int> ids);
        
    }