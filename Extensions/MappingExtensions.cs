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

    public static decimal Subtotal(this DetallePedido detalle) => detalle.Precio * detalle.Cantidad;

    public static DetallePedidoDto ToDto(this DetallePedido detalle) => new()
    {
        Id = detalle.Id,
        ProductoId = detalle.ProductoId,
        ProductoName = detalle.Producto?.Name ?? string.Empty,
        ProductoType = detalle.Producto?.Type ?? string.Empty,
        Cantidad = detalle.Cantidad,
        Precio = detalle.Precio,
        Subtotal = detalle.Subtotal(),
        Pagado = detalle.Pagado
    };

    public static PagoDto ToDto(this Pago pago) => new()
    {
        Id = pago.Id,
        PedidoId = pago.PedidoId,
        Importe = pago.Importe,
        Fecha = pago.Fecha,
        Metodo = pago.Metodo
    };

    public static PedidoDto ToDto(this Pedido pedido)
    {
        var total = pedido.Detalles.Sum(d => d.Subtotal());
        var pendiente = pedido.Detalles.Where(d => !d.Pagado).Sum(d => d.Subtotal());

        return new PedidoDto
        {
            Id = pedido.Id,
            MesaId = pedido.MesaId,
            MesaNumero = pedido.Mesa?.Numero ?? 0,
            UserId = pedido.UserId,
            Username = pedido.User?.username ?? string.Empty,
            Fecha = pedido.Fecha,
            Estado = pedido.Estado,
            Detalles = pedido.Detalles.OrderBy(d => d.Id).Select(d => d.ToDto()).ToList(),
            Total = total,
            TotalPagado = pedido.Pagos.Sum(p => p.Importe),
            Pendiente = pendiente
        };
    }
}
