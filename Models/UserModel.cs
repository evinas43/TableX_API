namespace TableX_API.Models
{
    public class UserModel
    {
        public int id { get; set; }

        //null!,inicia el valor en null para evitar errores de valor null y al iniciar la propiedad username, se le asigna un valor nulo de forma predeterminada.

        public string username { get; set; } = null!; 

        public string password { get; set; } = null!;

        public string role { get; set; }= null!;

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
