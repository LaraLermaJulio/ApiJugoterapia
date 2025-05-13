using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiJugoterapia.Models
{
    public class Jugo
    {
        public required int Id { get; set; }
        public required int Tipo { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public required string Ingredientes { get; set; }
        public required double Precio { get; set; }
        public required int Unidades { get; set; }
        public required bool Visible { get; set; } = true;

        [BindNever]
        public string? ImagenUrl { get; set; }


    }
}
