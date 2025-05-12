using ApiJugoterapia.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiJugoterapia.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Jugo> Jugos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<OrdenItem> OrdenItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones

            // Configuración de Jugo
            modelBuilder.Entity<Jugo>()
                .HasKey(j => j.Id);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>()
                .HasMany<Orden>(c => c.Ordenes)
                .WithOne(o => o.Cliente)
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de Orden
            modelBuilder.Entity<Orden>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Orden)
                .HasForeignKey(i => i.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de OrdenItem
            modelBuilder.Entity<OrdenItem>()
                .HasOne(oi => oi.Jugo)
                .WithMany()
                .HasForeignKey(oi => oi.JugoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}