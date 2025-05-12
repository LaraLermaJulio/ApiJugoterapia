using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiJugoterapia.Context;
using ApiJugoterapia.Models;

namespace ApiJugoterapia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JugosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _uploadPath;

        public JugosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _uploadPath = Path.Combine(_env.ContentRootPath, "uploads/images");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Jugo>> PostJugo([FromForm] Jugo jugo, IFormFile? imagen = null)
        {
            if (imagen != null && imagen.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                jugo.ImagenUrl = $"/images/{fileName}";
            }

            _context.Jugos.Add(jugo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJugo), new { id = jugo.Id }, jugo);
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> PutJugo(int id, [FromForm] Jugo jugo, IFormFile? imagen = null)
        {
            if (id != jugo.Id)
                return BadRequest("El ID proporcionado no coincide con el ID del jugo");

            if (imagen != null && imagen.Length > 0)
            {
                // Eliminar imagen anterior
                if (!string.IsNullOrEmpty(jugo.ImagenUrl))
                {
                    var oldImagePath = Path.Combine(_uploadPath, Path.GetFileName(jugo.ImagenUrl));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Guardar nueva imagen
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                jugo.ImagenUrl = $"/images/{fileName}";
            }

            _context.Entry(jugo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JugoExists(id))
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Jugo>>> GetJugos([FromQuery] int? tipo, [FromQuery] string? nombre, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            IQueryable<Jugo> query = _context.Jugos;

            if (tipo.HasValue)
            {
                query = query.Where(j => j.Tipo == tipo);
            }

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(j => j.Nombre.Contains(nombre));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalItems.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append("X-Current-Page", page.ToString());
            Response.Headers.Append("X-Total-Pages", Math.Ceiling((double)totalItems / pageSize).ToString());

            return items;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Jugo>> GetJugo(int id)
        {
            var jugo = await _context.Jugos.FindAsync(id);

            if (jugo == null)
            {
                return NotFound();
            }

            return jugo;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJugo(int id)
        {
            var jugo = await _context.Jugos.FindAsync(id);
            if (jugo == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(jugo.ImagenUrl))
            {
                var imagePath = Path.Combine(_uploadPath, Path.GetFileName(jugo.ImagenUrl));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Jugos.Remove(jugo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JugoExists(int id)
        {
            return _context.Jugos.Any(e => e.Id == id);
        }
    }
}
