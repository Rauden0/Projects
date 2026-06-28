using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Genre;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class GenreRepositoryTests
{
    private BookHubDbContext _dbContext = null!;
    private GenreRepository _genreRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var opt = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new BookHubDbContext(opt);
        _genreRepository = new GenreRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    #region BaseRepositoryTests
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var genre = new Genre { Name = "Horror"};
        _genreRepository.Add(genre);
        await _dbContext.SaveChangesAsync();
        
        var foundGenre = await _genreRepository.GetByIdAsync(1);
        Assert.That(foundGenre, Is.Not.Null);
        Assert.That(foundGenre.Name, Is.EqualTo("Horror"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnResult()
    {
        _dbContext.Genres.AddRange(
            new Genre { Name = "Science Fiction"},
            new Genre { Name = "Horror"}
        );
        await _dbContext.SaveChangesAsync();
        
        var genres = await _genreRepository.GetAllAsync();
        Assert.That(genres, Is.Not.Empty);
        Assert.That(genres, Has.Count.EqualTo(2));
        Assert.That(genres.Select(u => u.Name), Does.Contain("Science Fiction").And.Contain("Horror"));
    }

    [Test]
    public void UpdateAsync_ShouldWork()
    {
        var genre = new Genre { Name = "Science Fiction"};
        _dbContext.Genres.Add(genre);
        _dbContext.SaveChanges();
        
        genre.Name = "Sci-fi";
        _genreRepository.Update(genre);
        _dbContext.SaveChanges();
        
        var updatedGenre = _dbContext.Genres.Find(genre.Id);
        Assert.That(updatedGenre!.Name, Is.EqualTo("Sci-fi"));
    }

    [Test]
    public void RemoveAsync_ShouldSetIsRemoved()
    {
        var genre = new Genre { Name = "Science Fiction"};
        _dbContext.Genres.Add(genre);
        _dbContext.SaveChanges();
        
        _genreRepository.Remove(genre);
        _dbContext.SaveChanges();
        var removedGenre = _dbContext.Genres.Find(genre.Id);
        Assert.That(removedGenre!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnResult()
    {
        _dbContext.Genres.AddRange(
            new Genre { Name = "Science Fiction"},
            new Genre { Name = "Horror"}
        );
        _dbContext.SaveChanges();
        
        var genres = _genreRepository.Query().Where(u => u.Name.Contains("H"));
        Assert.That(genres.Single().Name, Is.EqualTo("Horror"));
    }
    #endregion
    
    #region GenreRepositoryTests

    [Test]
    public async Task GetGenreByIdAsync_ShouldWork()
    {
        var genre = new Genre { Name = "Science Fiction"};
        _dbContext.Genres.Add(genre);
        _dbContext.SaveChanges();
        
        var foundGenre = await _genreRepository.GetGenreByIdAsync(1);
        Assert.That(foundGenre, Is.Not.Null);
    }
    #endregion
}