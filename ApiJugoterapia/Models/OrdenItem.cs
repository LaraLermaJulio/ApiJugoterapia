namespace ApiJugoterapia.Models
{
    public class OrdenItem
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public required int JugoId { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }

        // Relaciones
        public virtual Orden? Orden { get; set; }
        public virtual Jugo? Jugo { get; set; }
        
    }
}
