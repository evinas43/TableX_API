using System.ComponentModel.DataAnnotations;
using TableX_API.Lib.Consts;

namespace TableX_API.DTOs;

public class CreateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 4)]
    public string Password { get; set; } = string.Empty;

    [Required, AllowedValues(Roles.Camarero, Roles.Caja, ErrorMessage = "El rol debe ser 'camarero' o 'caja'.")]
    public string Role { get; set; } = string.Empty;
}
