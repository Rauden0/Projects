
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BusinessLayer.Service;

public class ImageService : IImageService
{
    private readonly string _imageDirectory;

    public ImageService(string imageDirectory)
    {
        _imageDirectory = imageDirectory;
        if (!Directory.Exists(_imageDirectory)) Directory.CreateDirectory(_imageDirectory);
    }

    public async Task<string> SaveBookImageAsync(IFormFile image, string bookName)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("Invalid image file");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type");

        var fileName = $"book_{bookName}_{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_imageDirectory, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/images/books/{fileName}";
    }

    public void DeleteBookImageAsync(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;
        var fileName = Path.GetFileName(imagePath);
        var fullPath = Path.Combine(_imageDirectory, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

    }
    
    public async Task<string?> GetImageAsBase64Async(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;

        var fullPath = Path.Combine(_imageDirectory, Path.GetFileName(relativePath));

        if (!File.Exists(fullPath)) return null;

        var bytes = await File.ReadAllBytesAsync(fullPath);

        using var image = Image.Load(bytes);
    
        image.Mutate(x => x.Resize(new ResizeOptions {
            Size = new Size(400, 0),
            Mode = ResizeMode.Max
        }));

        using var ms = new MemoryStream();
        await image.SaveAsync(ms, new JpegEncoder { Quality = 75 });

        return Convert.ToBase64String(ms.ToArray());
    }

}