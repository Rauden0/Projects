using BusinessLayer.Service;
using BookHub.Controller;
using BusinessLayer.Dto.User;
using DataAccessLayer.Enums;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Moq;

namespace Tests.UnitTests.Controllers;

[TestFixture]
public class UserControllerTests
{
    private Mock<IUserService> _userServiceMock = null!;
    private UsersController _userController = null!;

    [SetUp]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();
        _userController = new UsersController(_userServiceMock.Object);
    }

    #region GetUser
    [Test]
    public async Task GetUser_ShouldReturnOk()
    {
        var userDto = new UserDto { Id = 1, DisplayName = "Vitr", Email = "vitr@email.com", Role = "1" };
        _userServiceMock.Setup(s => s.GetUserAsync(1)).ReturnsAsync(new Result<UserDto>(userDto));

        var result = await _userController.GetUserAsync(1);
        
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(userDto));
    }

    [Test]
    public async Task GetUser_ShouldReturnNotFound()
    {
        _userServiceMock.Setup(s => s.GetUserAsync(12)).ReturnsAsync(new Result<UserDto>(new Exception("User 12 not found")));
        
        var result = await _userController.GetUserAsync(12);
        
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("User 12 not found"));
    }
    #endregion
    
    #region GetUsers
    [Test]
    public async Task GetUsers_ShouldReturnOk_WithBooks()
    {
        var userList = new List<UserDto>
        {
            new UserDto { Id = 1, DisplayName = "admin", Email = "admin@email.com", Role = "2" },
            new UserDto { Id = 2, DisplayName = "Pepa", Email = "pepa@email.com", Role = "1" },
            new UserDto { Id = 3, DisplayName = "Vitr", Email = "vitr@email.com", Role = "1" }
        };
        _userServiceMock.Setup(s => s.GetUsersAsync(It.IsAny<ODataQueryOptions<UserDto>>()))
            .ReturnsAsync(new Result<List<UserDto>>(userList));

        var result = await _userController.GetUsers(null);

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.EqualTo(userList));
    }

    [Test]
    public async Task GetUsers_ShouldReturnNotFound_OnException()
    {
        _userServiceMock.Setup(s => s.GetUsersAsync(It.IsAny<ODataQueryOptions<UserDto>>())).ReturnsAsync(new Result<List<UserDto>>(new Exception("Error fetching users")));
        
        var result = await _userController.GetUsers(null);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFound = result as NotFoundObjectResult;
        Assert.That(notFound!.Value, Is.EqualTo("Error fetching users"));
    }
    #endregion
    
    #region AddUser
    [Test]
    public async Task AddUser_ShouldReturnOk()
    {
        var userRequest = new CreateUserDto { DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        var userDto = new UserDto {DisplayName = "Karel", Email = "karel@email.com", Role = "1", Id = 5};
        _userServiceMock.Setup(s => s.CreateAsync(userRequest)).ReturnsAsync(new Result<UserDto>(userDto));
        
        var result = await _userController.CreateAsync(userRequest);
        var ok = result as OkObjectResult;
        
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(userDto));
    }
    #endregion

    #region  UpdateUser
    [Test]
    public async Task UpdateUser_ShouldReturnOk()
    {
        var userRequest = new UpdateUserDto {Id = 1, DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        var userDto = new UserDto {DisplayName = "Karel", Email = "karel@email.com", Role = "1", Id = 1};
        _userServiceMock.Setup(s => s.UpdateAsync(1, userRequest)).ReturnsAsync(new Result<UserDto>(userDto));
        
        var result = await _userController.UpdateAsync(1, userRequest);
        
        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok.Value, Is.EqualTo(userDto));
    }

    [Test]
    public async Task UpdateUser_ShouldReturnNotFound_WhenMissing()
    {
        var userRequest = new UpdateUserDto {Id = 30, DisplayName = "Karel", Email = "karel@email.com", Role = RoleType.User, Password = "1234"};
        _userServiceMock.Setup(s => s.UpdateAsync(30, userRequest)).ReturnsAsync(new Result<UserDto>(new Exception("User 30 not found")));
        
        var result = await _userController.UpdateAsync(30, userRequest);
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
    #endregion
    #region DeleteUser

    [Test]
    public async Task DeleteUser_ShouldReturnNoContent()
    {
        _userServiceMock.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);
        var result = await _userController.Delete(1);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
    #endregion
}