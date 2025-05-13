using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ApiJugoterapia.Models
{
    public enum tipoUsuario
    {
        admin,
        cliente
    }

    [Index(nameof(Email), IsUnique = true)] 
    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public tipoUsuario TipoUsuario { get; set; } = tipoUsuario.cliente;

        [Required]
        public string Nombre { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public virtual ICollection<Orden>? Ordenes { get; set; }
    }
}
