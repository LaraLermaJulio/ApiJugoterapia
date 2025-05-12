using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiJugoterapia.Service
{
    public class ImageService : IImageService
    {
        private readonly string _uploadPath;
        private readonly string _baseUrl;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IConfiguration configuration, IWebHostEnvironment env, ILogger<ImageService> logger)
        {
            _env = env;
            _logger = logger;

            // Obtener la ruta base desde la configuración
            var uploadPath = configuration.GetSection("FileStorage")["UploadPath"] ?? "uploads/images";

            // Combinar con la ruta raíz del contenido
            _uploadPath = Path.Combine(_env.ContentRootPath, uploadPath);
            _baseUrl = "/images";

            // Asegurar que el directorio existe
            try
            {
                if (!Directory.Exists(_uploadPath))
                {
                    Directory.CreateDirectory(_uploadPath);
                    _logger.LogInformation($"Directorio creado: {_uploadPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"No se pudo crear el directorio de uploads: {_uploadPath}");
                throw; // Esto hará que la aplicación falle durante el inicio si no puede crear el directorio
            }
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    _logger.LogWarning("Intento de guardar imagen nula o vacía");
                    return null;
                }

                string[] allowedTypes = { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    _logger.LogWarning($"Tipo de archivo no permitido: {imageFile.ContentType}");
                    throw new ArgumentException("Tipo de archivo no permitido. Solo se aceptan imágenes JPG, PNG y GIF.");
                }

                // Crear directorio si no existe
                var fullPath = Path.Combine(_env.ContentRootPath, _uploadPath);
                _logger.LogInformation($"Ruta completa para imágenes: {fullPath}");

                if (!Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.CreateDirectory(fullPath);
                        _logger.LogInformation($"Directorio creado: {fullPath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error al crear directorio: {fullPath}");
                        throw new Exception($"No se pudo crear el directorio para guardar imágenes: {ex.Message}");
                    }
                }

                // Generar nombre único para la imagen
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(fullPath, fileName);
                _logger.LogInformation($"Guardando imagen en: {filePath}");

                // Guardar imagen
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    _logger.LogInformation($"Imagen guardada exitosamente: {fileName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al guardar imagen en: {filePath}");
                    throw new Exception($"Error al guardar la imagen: {ex.Message}");
                }

                // Devolver la URL relativa de la imagen
                return GetImageUrl(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al guardar imagen");
                throw;
            }
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    _logger.LogWarning("Intento de eliminar imagen con ruta vacía");
                    return false;
                }

                var fileName = Path.GetFileName(imagePath.Replace(_baseUrl, "").TrimStart('/'));
                var fullPath = Path.Combine(_env.ContentRootPath, _uploadPath, fileName);
                _logger.LogInformation($"Intentando eliminar imagen: {fullPath}");

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation($"Imagen eliminada: {fullPath}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Imagen no encontrada para eliminar: {fullPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar imagen: {imagePath}");
                return false;
            }
        }

        public string GetImageUrl(string imageName)
        {
            return $"{_baseUrl}/{imageName}";
        }
    }
}