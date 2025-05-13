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
        private readonly string _uploadPath;

        public JugosController(AppDbContext context)
        {
            _context = context;
            _uploadPath = @"C:\Users\ariza\Documentos\Proyects\Web\Jugoterapia\IMG";

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
                // Generar ID basado en fecha y hora
                var timestampId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var extension = Path.GetExtension(imagen.FileName); // incluye el punto: .jpg, .png, etc
                var fileName = $"{timestampId}{extension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // Guardar el nombre completo con extensión
                jugo.ImagenUrl = fileName; // ahora guarda "20250512_183000.jpg"
            }

            _context.Jugos.Add(jugo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJugo), new { id = jugo.Id }, jugo);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Jugo>>> GetJugos()
        {
            return await _context.Jugos.ToListAsync();
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
                var imagePath = Path.Combine(_uploadPath, $"{jugo.ImagenUrl}.png"); // Asumiendo .png
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Jugos.Remove(jugo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
