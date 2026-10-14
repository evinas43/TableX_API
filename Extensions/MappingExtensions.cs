using TableX_API.DTOs;
using TableX_API.Models;

namespace TableX_API.Extensions;

public static class MappingExtensions
{
    public static UserDto ToDto(this UserModel user) => new()
    {
        Id = user.id,
        Username = user.username,
        Role = user.role
    };

   
}
