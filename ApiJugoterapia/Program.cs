using ApiJugoterapia.Context;
using ApiJugoterapia.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Variable de conexión
var connectionstring = builder.Configuration.GetConnectionString("Connection");


// Solo configuración esencial
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionstring, ServerVersion.AutoDetect(connectionstring)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuración de archivos estáticos
var uploadPath = Path.Combine(app.Environment.ContentRootPath, "uploads/images");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/images"
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();
app.Run();