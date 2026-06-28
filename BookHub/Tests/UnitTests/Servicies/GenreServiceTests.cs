using BusinessLayer.Dto.Genre;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Genre;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class GenreServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IGenreRepository> _genreRepositoryMock = null!;
    private GenreService _genreService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _genreRepositoryMock = new Mock<IGenreRepository>();
        _uowMock.Setup(u => u.Genres).Returns(_genreRepositoryMock.Object);
        _genreService = new GenreService(_uowMock.Object);
    }

    #region GetGenre
    [Test]
    public async Task? GetGenre_ShouldReturnResult()
    {
        var genre = new Genre { Id = 1, Name = "Sci-fi"};
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);

        var result = await _genreService.GetGenreAsync(1);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.Name, e => e.Message), Is.EqualTo("Sci-fi"));
    }

    [Test]
    public async Task GetGenre_ShouldReturnError()
    {
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(30)).ReturnsAsync((Genre?)null);
        var result = await _genreService.GetGenreAsync(30);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Genre 30 not found"));
    }
    #endregion

    #region AddGenre
    [Test]
    public async Task AddGenre_ShouldReturnOk()
    {
        var genreRequest = new CreateGenreDto {Name = "Horror"};
        var genre = new Genre {Id = 1, Name = "Horror"};
        _genreRepositoryMock.Setup(r => r.Add(genre));
        
        var result = await _genreService.CreateAsync(genreRequest);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.Name, e => e.Message), Is.EqualTo("Horror"));
        _genreRepositoryMock.Verify(r => r.Add(It.IsAny<Genre>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion
    
    #region UpdateGenre
    [Test]
    public async Task UpdateGenre_ShouldReturnOk()
    {
        var genreRequest = new UpdateGenreDto { Name = "Horror"};
        var genre = new Genre {Id = 1, Name = "Horor"};
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);
        
        var result = await _genreService.UpdateAsync(1, genreRequest);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(genre.Name, Is.EqualTo("Horror"));
        _genreRepositoryMock.Verify(r => r.Update(genre), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateGenre_ShouldReturnError()
    {
        var genreRequest = new UpdateGenreDto { Name = "Sci-fi"};
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(55)).ReturnsAsync((Genre?)null);
        
        var result = await _genreService.UpdateAsync(55, genreRequest);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("Genre 55 not found"));
    }
    #endregion
    
    #region DeleteGenre
    [Test]
    public async Task DeleteGenre_ShouldRemoveUser()
    {
        var genre = new Genre {Id = 1, Name = "Sci-fi"};
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(genre);
        
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<Task>>(),
            null
        )).Returns<Func<Task>, Func<Task<bool>>?>(async (op, verify) => await op());
        
        await _genreService.DeleteAsync(1);
        
        _genreRepositoryMock.Verify(r =>r.Remove(genre), Times.Once);
        _uowMock.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
    }

    [Test]
    public async Task DeleteGenre_ShouldReturnError()
    {
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<Task>>(),
            null
        )).Returns<Func<Task>, Func<Task<bool>>?>(async (op, _) => await op());
        
        await _genreService.DeleteAsync(1);
        _genreRepositoryMock.Verify(r =>r.Remove(It.IsAny<Genre>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion
}