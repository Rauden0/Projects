using api.Dtos.User;
using TournamentManagaer.Entities;

namespace api.Mappers;

public static class UserMapper
{
    public static User ToUserFromCreateDto(this CreateUserDto userDto)
    {
        return new User
        {
            FirstName = userDto.FirstName!,
            LastName = userDto.LastName!
        };
    }

    public static User ToUserFromUpdateDto(this UpdateUserDto userDto)
    {
        return new User
        {
            FirstName = userDto.FirstName!,
            LastName = userDto.LastName!
        };
    }
}