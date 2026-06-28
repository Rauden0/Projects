using BusinessLayer.Dto.User;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Enums;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    public UserService(IUnitOfWork uow) { _uow = uow; }

    public async Task<Result<List<UserDto>>> GetUsersAsync(ODataQueryOptions<UserDto>? options)
    {
        return await UserMapper.ProjectToDto(_uow.Users.Query()).ApplyIfNotNull(options).ToListAsync();
    }

    public async Task<Result<UserDto>> GetUserAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        return user is not null ? UserMapper.ToDto(user) : new Result<UserDto>(new Exception(($"User {id} not found")));
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email)
    {
        var user = await _uow.Users.GetUserByEmailAsync(email);
        return user is not null ? UserMapper.ToDto(user) : new Result<UserDto>(new Exception(($"User {email} not found")));
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto req)
    {
        var user = new User
        {
            DisplayName = req.DisplayName,
            Email = req.Email,
            PasswordHash = req.Password,
            Role = req.Role ?? RoleType.User
        };
        _uow.Users.Add(user);
        await _uow.SaveChangesAsync();
        return UserMapper.ToDto(user);
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto req)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user is null)
        {
            return new Result<UserDto>(new Exception($"User {id} not found"));
        }
        user.DisplayName = req.DisplayName;
        user.Email = req.Email;
        user.Role = RoleType.User;
        user.PasswordHash = req.Password;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return UserMapper.ToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user != null)
                _uow.Users.Remove(user);
            await _uow.SaveChangesAsync();
        });
    }
}