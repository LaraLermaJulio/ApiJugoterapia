using ApiJugoterapia.DTOs;

namespace ApiJugoterapia.Models
{
    public class OrdenRequest
    {
        public int ClienteId { get; set; }
        public TipoEntrega TipoEntrega { get; set; }
        public string? DireccionEntrega { get; set; }
        public List<OrdenItemCreateDTO> Items { get; set; } = new List<OrdenItemCreateDTO>();

    }
}
