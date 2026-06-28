using BookHub.Controller;
using BusinessLayer.Dto.Genre;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class GenreControllerTests
{
    private Mock<IGenreService> _genreServiceMock = null!;
    private GenresController _genreController = null!;

    [SetUp]
    public void Setup()
    {
        _genreServiceMock = new Mock<IGenreService>();
        _genreController = new GenresController(_genreServiceMock.Object);
    }

    #region GetGenre
    [Test]
    public async Task GetGenre_ShouldReturnOk()
    {
        var genreDto = new GenreDto { Id = 1, Name = "Sci-fi" };
        _genreServiceMock.Setup(s => s.GetGenreAsync(1))
            .ReturnsAsync(new Result<GenreDto>(genreDto));

        var result = await _genreController.GetGenreAsync(1);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(genreDto));
    }

    [Test]
    public async Task GetGenre_ShouldReturnNotFound()
    {
        _genreServiceMock.Setup(s => s.GetGenreAsync(12))
            .ReturnsAsync(new Result<GenreDto>(new Exception("Genre 12 not found")));

        var result = await _genreController.GetGenreAsync(12);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("Genre 12 not found"));
    }
    #endregion

    #region GetGenres
    [Test]
    public async Task GetGenres_ShouldReturnOk()
    {
        var genreList = new List<GenreDto>
        {
            new GenreDto { Id = 1, Name = "Sci-fi" },
            new GenreDto { Id = 2, Name = "Poezie" },
            new GenreDto { Id = 3, Name = "Horror" }
        };

        _genreServiceMock.Setup(s => s.GetGenres(It.IsAny<ODataQueryOptions<GenreDto>>()))
            .ReturnsAsync(new Result<List<GenreDto>>(genreList));

        var result = await _genreController.GetGenres(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(genreList));
    }

    [Test]
    public async Task GetGenres_ShouldReturnNotFound_OnException()
    {
        _genreServiceMock.Setup(s => s.GetGenres(It.IsAny<ODataQueryOptions<GenreDto>>()))
            .ReturnsAsync(new Result<List<GenreDto>>(new Exception("Error fetching genres")));

        var result = await _genreController.GetGenres(null);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("Error fetching genres"));
    }
    #endregion

    #region AddGenre
    [Test]
    public async Task AddGenre_ShouldReturnOkOrCreated()
    {
        var genreRequest = new CreateGenreDto { Name = "Sci-fi" };
        var genreDto = new GenreDto { Id = 1, Name = "Sci-fi" };

        _genreServiceMock
            .Setup(s => s.CreateAsync(genreRequest))
            .ReturnsAsync(new Result<GenreDto>(genreDto));

        var result = await _genreController.Create(genreRequest); // IActionResult

        // OkObjectResult
        if (result is OkObjectResult ok)
        {
            Assert.That(ok.Value, Is.EqualTo(genreDto));
            return;
        }

        // CreatedAtActionResult
        if (result is CreatedAtActionResult created)
        {
            Assert.That(created.Value, Is.EqualTo(genreDto));
            return;
        }

        Assert.Fail($"Unexpected result type: {result.GetType().Name}");
    }

    [Test]
    public async Task AddGenre_ShouldReturnBadRequest_OnException()
    {
        var genreRequest = new CreateGenreDto { Name = "Sci-fi" };

        _genreServiceMock
            .Setup(s => s.CreateAsync(genreRequest))
            .ReturnsAsync(new Result<GenreDto>(new Exception("Create failed")));

        var result = await _genreController.Create(genreRequest); // IActionResult

        var bad = result as BadRequestObjectResult;
        Assert.That(bad, Is.Not.Null);
        Assert.That(bad!.Value, Is.EqualTo("Create failed"));
    }

    #endregion

    #region UpdateGenre
    [Test]
    public async Task UpdateGenre_ShouldReturnOk()
    {
        var genreRequest = new UpdateGenreDto { Name = "Horror" };
        var genreDto = new GenreDto { Name = "Horor" };

        _genreServiceMock.Setup(s => s.UpdateAsync(1, genreRequest))
            .ReturnsAsync(new Result<GenreDto>(genreDto));

        var result = await _genreController.Update(1, genreRequest);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(genreDto));
    }

    [Test]
    public async Task UpdateGenre_ShouldReturnNotFound_WhenMissing()
    {
        var genreRequest = new UpdateGenreDto { Name = "Sci-fi" };

        _genreServiceMock.Setup(s => s.UpdateAsync(30, genreRequest))
            .ReturnsAsync(new Result<GenreDto>(new Exception("Genre 30 not found")));

        var result = await _genreController.Update(30, genreRequest);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
    #endregion

    #region DeleteGenre
    [Test]
    public async Task DeleteGenre_ShouldReturnNoContentOrOk_WhenDeleted()
    {
        _genreServiceMock.Setup(s => s.CanDeleteGenreAsync(1))
            .ReturnsAsync(new Result<bool>(true));

        _genreServiceMock.Setup(s => s.DeleteGenreAsync(1))
            .ReturnsAsync(new Result<Unit>(Unit.Default));

        var result = await _genreController.Delete(1);
        
        if (result is NoContentResult)
        {
            Assert.Pass();
        }

        Assert.That(result, Is.TypeOf<OkResult>());
    }

    [Test]
    public async Task DeleteGenre_ShouldReturnConflict_WhenCannotDelete()
    {
        _genreServiceMock.Setup(s => s.CanDeleteGenreAsync(1))
            .ReturnsAsync(new Result<bool>(false));
        
        _genreServiceMock.Setup(s => s.DeleteGenreAsync(1))
            .ReturnsAsync(new Result<LanguageExt.Unit>(
                new Exception("Genre cannot be deleted because it is used by at least one book.")));

        var result = await _genreController.Delete(1);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());

        var conflict = result as ConflictObjectResult;
        Assert.That(conflict, Is.Not.Null);
        Assert.That(conflict!.Value, Is.EqualTo("Genre cannot be deleted because it is used by at least one book."));
    }


    [Test]
    public async Task DeleteGenre_ShouldReturnNotFound_WhenMissing()
    {
        _genreServiceMock.Setup(s => s.CanDeleteGenreAsync(999))
            .ReturnsAsync(new Result<bool>(new Exception("Genre 999 not found")));

        _genreServiceMock.Setup(s => s.DeleteGenreAsync(999))
            .ReturnsAsync(new Result<Unit>(new Exception("Genre 999 not found")));

        var result = await _genreController.Delete(999);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var nf = result as NotFoundObjectResult;
        Assert.That(nf!.Value, Is.EqualTo("Genre 999 not found"));
    }
    #endregion
}
