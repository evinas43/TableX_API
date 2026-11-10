using System.Data;
using Microsoft.EntityFrameworkCore;
using TableX_API.DTOs;
using TableX_API.Data;
using TableX_API.Extensions;
using TableX_API.Lib.Consts;
using TableX_API.Models;

namespace TableX_API.Services;

public class PagoService
{
    private readonly AppDbContext _db;
    private readonly PedidoService _pedidoService;

    public PagoService(AppDbContext db, PedidoService pedidoService)
    {
        _db = db;
        _pedidoService = pedidoService;
    }

    public async Task<List<PagoDto>> GetPagosDePedidoAsync(int pedidoId, CancellationToken ct = default)
    {
        if (!await _db.Pedidos.AnyAsync(p => p.Id == pedidoId, ct))
            throw new NotFoundException(ErrorMessages.PedidoNotFound);

        var pagos = await _db.Pagos.AsNoTracking()
            .Where(p => p.PedidoId == pedidoId)
            .OrderBy(p => p.Fecha).ThenBy(p => p.Id)
            .ToListAsync(ct);

        return pagos.Select(p => p.ToDto()).ToList();
    }

    public async Task<PendienteDto> GetPendienteAsync(int pedidoId, CancellationToken ct = default)
    {
        var pedido = await _db.Pedidos.AsNoTracking()
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Include(p => p.Pagos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == pedidoId, ct)
            ?? throw new NotFoundException(ErrorMessages.PedidoNotFound);

        var pendientes = pedido.Detalles.Where(d => !d.Pagado).OrderBy(d => d.Id).ToList();

        return new PendienteDto
        {
            PedidoId = pedido.Id,
            Total = pedido.Detalles.Sum(d => d.Subtotal()),
            TotalPagado = pedido.Pagos.Sum(p => p.Importe),
            Pendiente = pendientes.Sum(d => d.Subtotal()),
            DetallesPendientes = pendientes.Select(d => d.ToDto()).ToList()
        };
    }

    public async Task<PagoRegistradoDto> RegistrarPagoAsync(int pedidoId, CreatePagoRequest request, CancellationToken ct = default)
    {
        var (pago, detallesPagados) = await _db.ExecuteInTransactionAsync(async token =>
        {
            var pedido = await _db.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == pedidoId, token)
                ?? throw new NotFoundException(ErrorMessages.PedidoNotFound);

            PedidoService.EnsureAbierto(pedido);

            List<DetallePedido> lineas;

            if (request.DetalleIds is null || request.DetalleIds.Count == 0)
            {
                lineas = pedido.Detalles.Where(d => !d.Pagado).ToList();
            }
            else
            {
                var ids = request.DetalleIds.Distinct().ToList();
                lineas = pedido.Detalles.Where(d => ids.Contains(d.Id)).ToList();

                if (lineas.Count != ids.Count)
                    throw new BadRequestException(ErrorMessages.DetallesNotInPedido);
                if (lineas.Any(d => d.Pagado))
                    throw new ConflictException(ErrorMessages.DetallesAlreadyPaid);
            }

            var importe = lineas.Sum(d => d.Subtotal());
            if (importe <= 0)
                throw new ConflictException(ErrorMessages.NothingToPay);

            foreach (var linea in lineas)
                linea.Pagado = true;

            var nuevoPago = new Pago
            {
                PedidoId = pedidoId,
                Importe = importe,
                Metodo = request.Metodo
            };

            _db.Pagos.Add(nuevoPago);
            await _db.SaveChangesAsync(token);
            return (nuevoPago.ToDto(), lineas.Select(d => d.Id).OrderBy(id => id).ToList());
        }, IsolationLevel.Serializable, ct);

        return new PagoRegistradoDto
        {
            Pago = pago,
            DetallesPagados = detallesPagados,
            Pedido = await _pedidoService.GetByIdAsync(pedidoId, ct)
        };
    }
}
