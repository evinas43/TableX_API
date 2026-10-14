using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TableX_API.DTOs;
using TableX_API.Data;
using TableX_API.Extensions;
using TableX_API.Lib.Consts;
using TableX_API.Models;

namespace TableX_API.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    { 
        
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.username).ToListAsync(ct);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.id == id, ct)
            ?? throw new NotFoundException(ErrorMessages.UserNotFound);
        return user.ToDto();
    }

    public Task<UserDto> CreateAsync(CreateUserRequest request, ClaimsPrincipal caller, CancellationToken ct = default)
    {
        return _db.ExecuteInTransactionAsync(async token =>
        {
            var isBootstrap = !await _db.Users.AnyAsync(token);

            if (!isBootstrap)
            {
                if (caller.Identity?.IsAuthenticated != true)
                    throw new UnauthorizedException(ErrorMessages.MustBeLoggedIn);
                if (!caller.IsInRole(Roles.Caja))
                    throw new ForbiddenException(ErrorMessages.ForbiddenAction);
            }
            else if (request.Role != Roles.Caja)
            {
                throw new BadRequestException("El primer usuario del sistema debe tener el rol 'caja'.");
            }

            if (await _db.Users.AnyAsync(u => u.username == request.Username, token))
                throw new ConflictException(ErrorMessages.UsernameAlreadyExists);

            var user = new UserModel
            {
                username = request.Username,
                password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                role = request.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(token);

            return user.ToDto();
        }, IsolationLevel.Serializable, ct);
    }
}
