using BubuTrackerAPI.Dtos;
using BubuTrackerAPI.Repository;
using BubuTrackerAPI.UserDatabase.Models;
using Microsoft.AspNetCore.Mvc;

namespace BubuTrackerAPI.Controllers;
[Route("api/user")]
[ApiController]
public class UserController: ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task<IActionResult> LoginUser(UserLoginDto userLoginDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }
        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(userLoginDto.Password, user.Password);
        if (!isPasswordCorrect)
        {
            return Unauthorized(new { message = "Invalid password" });
        }
        return Ok(new { message = "Login successful" });
        
    }
    [HttpPost]
    public async Task<IActionResult> RegisterUser(UserRegisterDto registerDto)
    {
        User user = UserRegisterDto.UserRegisterDtoToUser(registerDto);
        user.Password = PasswordHelper.HashPassword(user.Password);
        return Ok(await _userRepository.CreateUserAsync(user));
    }
    
    public async Task<IActionResult> UpdateUser(UserUpdateDto userUpdateDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(userUpdateDto.Email);
        if (user == null) return NotFound();    

        user.FirstName = userUpdateDto.FirstName ?? "";
        user.LastName = userUpdateDto.LastName ?? "";

        await _userRepository.UpdateUserAsync(user);
        return Ok(user);
    }
    
}
