using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiJugoterapia.Context;
using ApiJugoterapia.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.CodeAnalysis.Scripting;

namespace ApiJugoterapia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/Registro
        [HttpPost("Registro")]
        public async Task<ActionResult<Cliente>> Registro(RegistroRequest request)
        {
            // Verificar si el email ya está registrado
            if (await _context.Clientes.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest("El correo electrónico ya está registrado");
            }

            // Crear nuevo cliente
            var cliente = new Cliente
            {
                Nombre = request.Nombre,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // Hashear la contraseña
                Telefono = request.Telefono,
                Direccion = request.Direccion
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Generar token JWT
            var token = GenerateJwtToken(cliente);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = cliente.Id,
                Nombre = cliente.Nombre
            });
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login(Models.LoginRequest request)
        {
            // Buscar usuario por email
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == request.Email);

            // Verificar si el usuario existe y la contraseña es correcta
            if (cliente == null || !BCrypt.Net.BCrypt.Verify(request.Password, cliente.Password))
            {
                return Unauthorized("Credenciales inválidas");
            }

            // Generar token JWT
            var token = GenerateJwtToken(cliente);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = cliente.Id,
                Nombre = cliente.Nombre
            });
        }

        private string GenerateJwtToken(Cliente cliente)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:SecretKey"] ?? "jugoterapia_default_key_must_be_at_least_32_chars"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, cliente.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, cliente.Email),
                new Claim("name", cliente.Nombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"] ?? "jugoterapia",
                audience: _configuration["JwtSettings:Audience"] ?? "jugoterapia_clients",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}