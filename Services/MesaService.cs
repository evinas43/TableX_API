using Microsoft.EntityFrameworkCore;
using TableX_API.Data;
using TableX_API.DTOs;
using TableX_API.Lib.Consts;
using TableX_API.Models;

namespace TableX_API.Services;

public class MesaService
{
    private readonly AppDbContext _db;

    public MesaService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<MesaDto>> GetAllAsync(CancellationToken ct = default)
    {
        return ProjectToDto(_db.Mesas.AsNoTracking())
            .OrderBy(m => m.Numero)
            .ToListAsync(ct);
    }

    public async Task<MesaDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await ProjectToDto(_db.Mesas.AsNoTracking().Where(m => m.Id == id))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(ErrorMessages.MesaNotFound);
    }

    public async Task<MesaDto> CreateAsync(MesaRequest request, CancellationToken ct = default)
    {
        var numero = request.Numero!.Value;

        if (await _db.Mesas.AnyAsync(m => m.Numero == numero, ct))
            throw new ConflictException(ErrorMessages.MesaNumeroAlreadyExists);

        var mesa = new Mesa { Numero = numero };
        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync(ct);

        return new MesaDto { Id = mesa.Id, Numero = mesa.Numero };
    }

    public async Task<MesaDto> UpdateAsync(int id, MesaRequest request, CancellationToken ct = default)
    {
        var numero = request.Numero!.Value;

        var mesa = await _db.Mesas.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException(ErrorMessages.MesaNotFound);

        if (await _db.Mesas.AnyAsync(m => m.Numero == numero && m.Id != id, ct))
            throw new ConflictException(ErrorMessages.MesaNumeroAlreadyExists);

        mesa.Numero = numero;
        await _db.SaveChangesAsync(ct);

        return new MesaDto { Id = mesa.Id, Numero = mesa.Numero };
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var mesa = await _db.Mesas.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException(ErrorMessages.MesaNotFound);

        _db.Mesas.Remove(mesa);
        await _db.SaveChangesAsync(ct);
    }

    private static IQueryable<MesaDto> ProjectToDto(IQueryable<Mesa> query) =>
        query.Select(m => new MesaDto
        {
            Id = m.Id,
            Numero = m.Numero
        });
}
