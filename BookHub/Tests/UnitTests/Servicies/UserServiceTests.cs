using BusinessLayer.Dto.User;
using BusinessLayer.Service;
using DataAccessLayer;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using DataAccessLayer.Repository.User;
using Moq;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUnitOfWork> _uowMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private UserService _userService = null!;

    [SetUp]
    public void SetUp()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _uowMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _userService = new UserService(_uowMock.Object);
    }

    #region GetUser
    [Test]
    public async Task? GetUser_ShouldReturnResult()
    {
        var user = new User { Id = 1, DisplayName = "Vita", Email = "vita@email.com", IsRemoved = false};
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await _userService.GetUserAsync(1);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.DisplayName, e => e.Message), Is.EqualTo("Vita"));
        Assert.That(result.Match(m => m.Email, e => e.Message), Is.EqualTo("vita@email.com"));
    }

    [Test]
    public async Task GetUser_ShouldReturnError()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(30)).ReturnsAsync((User?)null);
        var result = await _userService.GetUserAsync(30);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("User 30 not found"));
    }
    #endregion

    #region AddUser
    [Test]
    public async Task AddUser_ShouldReturnOk()
    {
        var userRequest = new CreateUserDto
            {DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        var user = new User {Id = 1, DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, IsRemoved = false};
        _userRepositoryMock.Setup(r => r.Add(user));
        
        var result = await _userService.CreateAsync(userRequest);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Match(m => m.DisplayName, e => e.Message), Is.EqualTo("Karel"));
        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion
    
    #region UpdateUser
    [Test]
    public async Task UpdateUser_ShouldReturnOk()
    {
        var userRequest = new UpdateUserDto { Id = 1, DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        var user = new User {Id = 1, DisplayName = "Vita", Email = "karel@email.com", IsRemoved = false};
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        
        var result = await _userService.UpdateAsync(1, userRequest);
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.DisplayName, Is.EqualTo("Karel"));
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task UpdateUser_ShouldReturnError()
    {
        var userRequest = new UpdateUserDto { Id = 55, DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        _userRepositoryMock.Setup(r => r.GetByIdAsync(55)).ReturnsAsync((User?)null);
        
        var result = await _userService.UpdateAsync(55, userRequest);
        
        Assert.That(result.IsFaulted, Is.True);
        Assert.That(result.ToString(), Does.Contain("User 55 not found"));
    }
    #endregion
    
    #region DeleteUser
    [Test]
    public async Task DeleteUser_ShouldRemoveUser()
    {
        var user = new User {Id = 1, DisplayName = "Vita", Email = "vita@email.com", IsRemoved = false};
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<Task>>(),
            null
        )).Returns<Func<Task>, Func<Task<bool>>?>(async (op, verify) => await op());
        
        await _userService.DeleteAsync(1);
        
        _userRepositoryMock.Verify(r =>r.Remove(user), Times.Once);
        _uowMock.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), null), Times.Once);
    }

    [Test]
    public async Task DeleteUser_ShouldReturnError()
    {
        _uowMock.Setup(u => u.ExecuteInTransactionAsync(
            It.IsAny<Func<Task>>(),
            null
        )).Returns<Func<Task>, Func<Task<bool>>?>(async (op, _) => await op());
        
        await _userService.DeleteAsync(1);
        _userRepositoryMock.Verify(r =>r.Remove(It.IsAny<User>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    #endregion
}