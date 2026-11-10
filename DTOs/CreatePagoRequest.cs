using System.ComponentModel.DataAnnotations;
using TableX_API.Lib.Consts;

namespace TableX_API.DTOs;

public class CreatePagoRequest
{
    [Required, AllowedValues(MetodosPago.Efectivo, MetodosPago.Tarjeta, ErrorMessage = "El método debe ser 'efectivo' o 'tarjeta'.")]
    public string Metodo { get; set; } = string.Empty;

    public List<int>? DetalleIds { get; set; }
}
