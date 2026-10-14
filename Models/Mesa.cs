namespace TableX_API.Models;

public class Mesa
{
    public int Id { get; set; }
    public int Numero { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
