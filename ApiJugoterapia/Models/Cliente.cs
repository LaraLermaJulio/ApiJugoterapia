namespace ApiJugoterapia.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public virtual ICollection<Orden>? Ordenes { get; set; }

    }
}
