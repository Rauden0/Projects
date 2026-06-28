using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.Book;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class BookRepositoryTest
{
    private BookHubDbContext _context = null!;
    private BookRepository _repo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookHubDbContext(options);
        _repo = new BookRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region Base Repository Tests
    [Test]
    public async Task Add_And_GetByIdAsync_ShouldWork()
    {
        var book = new Book { Name = "Foundation" , Description = "A science fiction novel by Isaac Asimov."};
        _repo.Add(book);
        await _context.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(book.Id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Name, Is.EqualTo("Foundation"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        _context.Books.AddRange(
            new Book { Name = "Book1" , Description = "Description1" },
            new Book { Name = "Book2", Description = "Description2" }
        );
        await _context.SaveChangesAsync();

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
        Assert.That(all.Select(b => b.Name), Does.Contain("Book1").And.Contain("Book2"));
    }

    [Test]
    public void Update_ShouldModifyEntity()
    {
        var book = new Book { Name = "Old Name", Description = "Old Description" };
        _context.Books.Add(book);
        _context.SaveChanges();

        book.Name = "New Name";
        _repo.Update(book);
        _context.SaveChanges();

        var updated = _context.Books.Find(book.Id);
        Assert.That(updated!.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void Remove_ShouldSetIsRemoved()
    {
        var book = new Book { Name = "ToRemove", Description = "To be removed" };
        _context.Books.Add(book);
        _context.SaveChanges();

        _repo.Remove(book);
        _context.SaveChanges();

        var removed = _context.Books.Find(book.Id);
        Assert.That(removed!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnQueryable()
    {
        _context.Books.AddRange(
            new Book { Name = "Book1", Description = "Description1" },
            new Book { Name = "Book2", Description = "Description2" }
        );
        _context.SaveChanges();

        var query = _repo.Query().Where(b => b.Name.Contains("1"));
        Assert.That(query.Single().Name, Is.EqualTo("Book1"));
    }
    #endregion

    #region BookRepository Specific
    [Test]
    public async Task GetBookByIdwithAuthorAndGenre_ShouldIncludeRelations()
    {
        var author = new Author { Name = "Asimov" };
        var genre = new Genre { Name = "Sci-Fi" };
        var book = new Book { Name = "Foundation", Authors = new List<Author> { author }, Genres = new List<Genre> { genre }, Description = "A science fiction novel by Isaac Asimov." };

        _context.Books.Add(book);
        _context.SaveChanges();

        var result = await _repo.GetBookByIdwithAuthorAndGenre(book.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Authors.Single().Name, Is.EqualTo("Asimov"));
        Assert.That(result.Genres.Single().Name, Is.EqualTo("Sci-Fi"));
    }

    [Test]
    public async Task GetBooksByFilter_ShouldReturnFilteredBooks()
    {
        _context.Books.AddRange(
            new Book { Name = "Dune", Description = "A science fiction novel by Frank Herbert." },
            new Book { Name = "Foundation", Description = "A science fiction novel by Isaac Asimov." }
        );
        _context.SaveChanges();

        var filtered = await _repo.GetBooksByFilter(b => b.Name.Contains("Dune"));
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].Name, Is.EqualTo("Dune"));
    }

    [Test]
    public async Task GetAuthorsByListIds_ShouldReturnMatchingAuthors()
    {
        var a1 = new Author { Name = "Asimov" };
        var a2 = new Author { Name = "Orwell" };
        _context.Authors.AddRange(a1, a2);
        await _context.SaveChangesAsync();

        var result = await _repo.GetAuthorsByListIds(new List<int> { a1.Id });
        Assert.That(result.Single().Name, Is.EqualTo("Asimov"));
    }

    [Test]
    public async Task GetGenresByListIds_ShouldReturnMatchingGenres()
    {
        var g1 = new Genre { Name = "Sci-Fi" };
        var g2 = new Genre { Name = "Fantasy" };
        _context.Genres.AddRange(g1, g2);
        await _context.SaveChangesAsync();

        var result = await _repo.GetGenresByListIds(new List<int> { g2.Id });
        Assert.That(result.Single().Name, Is.EqualTo("Fantasy"));
    }

    [Test]
    public async Task GetBooksByFilter_ShouldReturnEmpty_WhenNoMatch()
    {
        _context.Books.Add(new Book { Name = "Dune", Description = "A science fiction novel by Frank Herbert." });
        _context.SaveChanges();

        var filtered = await _repo.GetBooksByFilter(b => b.Name.Contains("Nonexistent"));
        Assert.That(filtered, Is.Empty);
    }
    #endregion
}