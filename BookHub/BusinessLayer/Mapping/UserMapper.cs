using BusinessLayer.Dto.User;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class UserMapper
{
    public static UserDto ToDto(User entity) =>
        EntityMapper.ToDto<User, UserDto>(entity);

    public static IQueryable<UserDto> ProjectToDto(IQueryable<User> query) =>
        EntityMapper.ProjectToDto<User, UserDto>(query);
}