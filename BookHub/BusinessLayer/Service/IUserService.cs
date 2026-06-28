using BusinessLayer.Dto.User;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IUserService
{
    Task<Result<List<UserDto>>> GetUsersAsync(ODataQueryOptions<UserDto>? options);
    Task<Result<UserDto>> GetUserAsync(int id);
    Task<Result<UserDto>> GetUserByEmailAsync(string email);
    Task<Result<UserDto>> CreateAsync(CreateUserDto req);
    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto req);
    Task DeleteAsync(int id);
}