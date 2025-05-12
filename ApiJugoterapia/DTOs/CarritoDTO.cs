// JugoDTO.cs
namespace ApiJugoterapia.DTOs
{
    public class JugoDTO
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ingredientes { get; set; } = string.Empty;
        public double Precio { get; set; }
        public double Unidades { get; set; }
        public string? ImagenUrl { get; set; }
    }

    // Para crear o actualizar un jugo
    public class JugoCreateUpdateDTO
    {
        public int Tipo { get; set; }
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public required string Ingredientes { get; set; }
        public double Precio { get; set; }
        public double Unidades { get; set; }
        // La imagen se maneja por separado como IFormFile
    }
}

// ClienteDTO.cs
namespace ApiJugoterapia.DTOs
{
    // DTO para listar clientes
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }

    // DTO para crear/actualizar clientes
    public class ClienteCreateUpdateDTO
    {
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }
}

// OrdenDTO.cs
namespace ApiJugoterapia.DTOs
{
    public class OrdenDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public DateTime FechaOrden { get; set; }
        public double Total { get; set; }
        public string TipoEntrega { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public List<OrdenItemDTO> Items { get; set; } = new List<OrdenItemDTO>();
    }

    public class OrdenItemDTO
    {
        public int Id { get; set; }
        public int JugoId { get; set; }
        public string? JugoNombre { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
    }

    // DTO mejorado para crear órdenes
    public class OrdenCreateDTO
    {
        public int ClienteId { get; set; }
        public string TipoEntrega { get; set; } = "Recoger"; // Valor predeterminado
        public string? DireccionEntrega { get; set; }
        public List<OrdenItemCreateDTO> Items { get; set; } = new List<OrdenItemCreateDTO>();
    }

    public class OrdenItemCreateDTO
    {
        public int JugoId { get; set; }
        public int Cantidad { get; set; }
    }

    // DTO para actualizar estado
    public class ActualizarEstadoDTO
    {
        public string Estado { get; set; } = string.Empty;
    }
}