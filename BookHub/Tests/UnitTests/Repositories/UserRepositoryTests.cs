using DataAccessLayer.Data;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.User;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    private BookHubDbContext _dbContext = null!;
    private UserRepository _userRepository = null!;

    [SetUp]
    public void SetUp()
    {
        var opt = new DbContextOptionsBuilder<BookHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new BookHubDbContext(opt);
        _userRepository = new UserRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    #region BaseRepositoryTests
    [Test]
    public async Task AddAsync_And_GetByIdAsync_ShouldWork()
    {
        var user = new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "dsds"};
        _userRepository.Add(user);
        await _dbContext.SaveChangesAsync();
        
        var foundUser = await _userRepository.GetByIdAsync(1);
        Assert.That(foundUser, Is.Not.Null);
        Assert.That(foundUser.DisplayName, Is.EqualTo("Vita"));
        Assert.That(foundUser.Email, Is.EqualTo("vita@email.com"));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnResult()
    {
        _dbContext.Users.AddRange(
            new User { DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, IsRemoved = false, PasswordHash = "dsds"},
            new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "ssss"}
        );
        await _dbContext.SaveChangesAsync();
        
        var users = await _userRepository.GetAllAsync();
        Assert.That(users, Is.Not.Empty);
        Assert.That(users.Count(), Is.EqualTo(2));
        Assert.That(users.Select(u => u.DisplayName), Does.Contain("Vita").And.Contain("Karel"));
    }

    [Test]
    public void UpdateAsync_ShouldWork()
    {
        var user = new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "ssss"};
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        
        user.DisplayName = "Vitr";
        _userRepository.Update(user);
        _dbContext.SaveChanges();
        
        var updatedUser = _dbContext.Users.Find(user.Id);
        Assert.That(updatedUser!.DisplayName, Is.EqualTo("Vitr"));
    }

    [Test]
    public void RemoveAsync_ShouldSetIsRemoved()
    {
        var user = new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "ssss"};
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        
        _userRepository.Remove(user);
        _dbContext.SaveChanges();
        var removedUser = _dbContext.Users.Find(user.Id);
        Assert.That(removedUser!.IsRemoved, Is.True);
    }

    [Test]
    public void Query_ShouldReturnResult()
    {
        _dbContext.Users.AddRange(
            new User { DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, IsRemoved = false, PasswordHash = "dsds"},
            new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "ssss"}
        );
        _dbContext.SaveChanges();
        
        var users = _userRepository.Query().Where(u => u.DisplayName.Contains("V"));
        Assert.That(users.Single().DisplayName, Is.EqualTo("Vita"));
    }
    #endregion
    
    #region UserRepositoryTests

    [Test]
    public async Task GetUserByIdAsync_ShouldWork()
    {
        var user = new User { DisplayName = "Vita", Email = "vita@email.com",Role = RoleType.User, IsRemoved = false, PasswordHash = "ssss"};
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        
        var foundUser = await _userRepository.GetUserByEmailAsync("vita@email.com");
        Assert.That(foundUser, Is.Not.Null);
    }
    #endregion
}