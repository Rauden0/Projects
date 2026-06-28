using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Service;

public interface IImageService
{
    Task<string> SaveBookImageAsync(IFormFile image, string bookName);
    void DeleteBookImageAsync(string imagePath);
    
    Task<string?> GetImageAsBase64Async(string? imagePath);
    
    
}
