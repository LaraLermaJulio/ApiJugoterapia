using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ApiJugoterapia.Service
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageFile);
        bool DeleteImage(string imagePath);
        string GetImageUrl(string imageName);
    }
}