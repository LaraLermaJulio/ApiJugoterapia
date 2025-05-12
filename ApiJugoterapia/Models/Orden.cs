namespace ApiJugoterapia.Models
{
    public enum TipoEntrega
    {
        Recoger,
        Delivery
    }

    public enum EstadoOrden
    {
        Pendiente,
        Preparando,
        EnRuta,
        Entregado,
        Cancelado
    } 

    public class Orden
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaOrden { get; set; } = DateTime.Now;
        public double Total { get; set; }
        public TipoEntrega TipoEntrega { get; set; }
        public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
        public string? DireccionEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }

        // Relaciones
        public virtual Cliente? Cliente { get; set; }
        public virtual ICollection<OrdenItem>? Items { get; set; }

    }
}
