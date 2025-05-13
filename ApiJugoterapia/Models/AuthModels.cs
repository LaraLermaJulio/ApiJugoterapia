namespace ApiJugoterapia.Models
{
    public class RegistroRequest
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public tipoUsuario TipoUsuario { get; set; } = tipoUsuario.cliente;
    }
}