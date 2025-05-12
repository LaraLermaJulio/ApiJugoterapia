using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiJugoterapia.Context;
using ApiJugoterapia.Models;
using ApiJugoterapia.DTOs;

namespace ApiJugoterapia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdenesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ordenes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenDTO>>> GetOrdenes([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Consulta base con todas las relaciones necesarias
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                .ThenInclude(i => i.Jugo)
                .AsQueryable();

            // Total de registros para la paginación
            var totalItems = await query.CountAsync();

            // Aplicar paginación
            var ordenes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapear a DTO
            var ordenesDTO = ordenes.Select(o => new OrdenDTO
            {
                Id = o.Id,
                ClienteId = o.ClienteId,
                ClienteNombre = o.Cliente?.Nombre,
                FechaOrden = o.FechaOrden,
                Total = o.Total,
                TipoEntrega = o.TipoEntrega.ToString(),
                Estado = o.Estado.ToString(),
                DireccionEntrega = o.DireccionEntrega,
                FechaEntrega = o.FechaEntrega,
                Items = o.Items?.Select(i => new OrdenItemDTO
                {
                    Id = i.Id,
                    JugoId = i.JugoId,
                    JugoNombre = i.Jugo?.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList() ?? new List<OrdenItemDTO>()
            }).ToList();

            // Agregar headers para paginación
            Response.Headers.Add("X-Total-Count", totalItems.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());
            Response.Headers.Add("X-Current-Page", page.ToString());
            Response.Headers.Add("X-Total-Pages", Math.Ceiling((double)totalItems / pageSize).ToString());

            return ordenesDTO;
        }

        // GET: api/Ordenes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenDTO>> GetOrden(int id)
        {
            var orden = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                .ThenInclude(i => i.Jugo)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return NotFound();
            }

            // Mapear a DTO
            var ordenDTO = new OrdenDTO
            {
                Id = orden.Id,
                ClienteId = orden.ClienteId,
                ClienteNombre = orden.Cliente?.Nombre,
                FechaOrden = orden.FechaOrden,
                Total = orden.Total,
                TipoEntrega = orden.TipoEntrega.ToString(),
                Estado = orden.Estado.ToString(),
                DireccionEntrega = orden.DireccionEntrega,
                FechaEntrega = orden.FechaEntrega,
                Items = orden.Items?.Select(i => new OrdenItemDTO
                {
                    Id = i.Id,
                    JugoId = i.JugoId,
                    JugoNombre = i.Jugo?.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList() ?? new List<OrdenItemDTO>()
            };

            return ordenDTO;
        }

        // GET: api/Ordenes/Cliente/5
        [HttpGet("Cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<OrdenDTO>>> GetOrdenesByCliente(int clienteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Verificar que el cliente existe
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == clienteId);
            if (!clienteExiste)
            {
                return NotFound("Cliente no encontrado");
            }

            // Consulta base
            var query = _context.Ordenes
                .Include(o => o.Items)
                .ThenInclude(i => i.Jugo)
                .Where(o => o.ClienteId == clienteId)
                .AsQueryable();

            // Total de registros para la paginación
            var totalItems = await query.CountAsync();

            // Aplicar paginación
            var ordenes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapear a DTO
            var ordenesDTO = ordenes.Select(o => new OrdenDTO
            {
                Id = o.Id,
                ClienteId = o.ClienteId,
                FechaOrden = o.FechaOrden,
                Total = o.Total,
                TipoEntrega = o.TipoEntrega.ToString(),
                Estado = o.Estado.ToString(),
                DireccionEntrega = o.DireccionEntrega,
                FechaEntrega = o.FechaEntrega,
                Items = o.Items?.Select(i => new OrdenItemDTO
                {
                    Id = i.Id,
                    JugoId = i.JugoId,
                    JugoNombre = i.Jugo?.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList() ?? new List<OrdenItemDTO>()
            }).ToList();

            // Agregar headers para paginación
            Response.Headers.Add("X-Total-Count", totalItems.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());
            Response.Headers.Add("X-Current-Page", page.ToString());
            Response.Headers.Add("X-Total-Pages", Math.Ceiling((double)totalItems / pageSize).ToString());

            return ordenesDTO;
        }

        // POST: api/Ordenes
        [HttpPost]
        public async Task<ActionResult<OrdenDTO>> CrearOrden(OrdenRequest ordenRequest)
        {
            // Verificar que el cliente existe
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == ordenRequest.ClienteId);
            if (!clienteExiste)
            {
                return BadRequest("El cliente especificado no existe");
            }

            // Verificar que hay items en el pedido
            if (ordenRequest.Items == null || !ordenRequest.Items.Any())
            {
                return BadRequest("La orden debe contener al menos un ítem");
            }

            // Si es delivery, verificar que tenga dirección
            if (ordenRequest.TipoEntrega == TipoEntrega.Delivery && string.IsNullOrEmpty(ordenRequest.DireccionEntrega))
            {
                return BadRequest("Para el tipo de entrega Delivery se requiere una dirección");
            }

            // Verificar que todos los jugos existen y obtener sus precios actuales
            var jugoIds = ordenRequest.Items.Select(i => i.JugoId).ToList();
            var jugos = await _context.Jugos.Where(j => jugoIds.Contains(j.Id)).ToDictionaryAsync(j => j.Id, j => j);

            // Validar que todos los jugos existen
            foreach (var item in ordenRequest.Items)
            {
                if (!jugos.ContainsKey(item.JugoId))
                {
                    return BadRequest($"El jugo con ID {item.JugoId} no existe");
                }
            }

            // Crear la orden
            var orden = new Orden
            {
                ClienteId = ordenRequest.ClienteId,
                FechaOrden = DateTime.Now,
                TipoEntrega = ordenRequest.TipoEntrega,
                DireccionEntrega = ordenRequest.DireccionEntrega,
                Estado = EstadoOrden.Pendiente,
                Items = new List<OrdenItem>(),
            };

            // Crear los items de la orden y calcular el total
            double total = 0;
            foreach (var item in ordenRequest.Items)
            {
                var precioJugo = jugos[item.JugoId].Precio;
                var subtotal = precioJugo * item.Cantidad;
                total += subtotal;

                var ordenItem = new OrdenItem
                {
                    JugoId = item.JugoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precioJugo,
                    Subtotal = subtotal
                };

                orden.Items.Add(ordenItem);
            }

            orden.Total = total;

            // Guardar la orden
            _context.Ordenes.Add(orden);
            await _context.SaveChangesAsync();

            // Cargar la orden completa con relaciones
            var ordenCompleta = await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Items)
                .ThenInclude(i => i.Jugo)
                .FirstOrDefaultAsync(o => o.Id == orden.Id);

            // Mapear a DTO para la respuesta
            var ordenDTO = new OrdenDTO
            {
                Id = ordenCompleta.Id,
                ClienteId = ordenCompleta.ClienteId,
                ClienteNombre = ordenCompleta.Cliente?.Nombre,
                FechaOrden = ordenCompleta.FechaOrden,
                Total = ordenCompleta.Total,
                TipoEntrega = ordenCompleta.TipoEntrega.ToString(),
                Estado = ordenCompleta.Estado.ToString(),
                DireccionEntrega = ordenCompleta.DireccionEntrega,
                FechaEntrega = ordenCompleta.FechaEntrega,
                Items = ordenCompleta.Items?.Select(i => new OrdenItemDTO
                {
                    Id = i.Id,
                    JugoId = i.JugoId,
                    JugoNombre = i.Jugo?.Nombre,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    Subtotal = i.Subtotal
                }).ToList() ?? new List<OrdenItemDTO>()
            };

            return CreatedAtAction("GetOrden", new { id = orden.Id }, ordenDTO);
        }

        // PUT: api/Ordenes/ActualizarEstado/5
        [HttpPut("ActualizarEstado/{id}")]
        public async Task<IActionResult> ActualizarEstadoOrden(int id, [FromBody] ActualizarEstadoDTO actualizarEstadoDTO)
        {
            var orden = await _context.Ordenes.FindAsync(id);
            if (orden == null)
            {
                return NotFound();
            }

            // Validar y convertir el estado
            if (!Enum.TryParse<EstadoOrden>(actualizarEstadoDTO.Estado, out var nuevoEstado))
            {
                return BadRequest("Estado de orden no válido");
            }

            orden.Estado = nuevoEstado;

            // Si el estado es Entregado, actualizar la fecha de entrega
            if (nuevoEstado == EstadoOrden.Entregado)
            {
                orden.FechaEntrega = DateTime.Now;
            }

            _context.Entry(orden).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrdenExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool OrdenExists(int id)
        {
            return _context.Ordenes.Any(e => e.Id == id);
        }
    }
}