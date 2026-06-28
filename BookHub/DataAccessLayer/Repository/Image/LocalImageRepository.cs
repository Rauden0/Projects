using DataAccessLayer.Repository.Image;

public class LocalImageRepository : IImageRepository
{
    private readonly string _rootPath;

    public LocalImageRepository(string rootPath) => _rootPath = rootPath;

    public async Task<string> UploadAsync(Stream fileStream, string subDirectory, string fileName)
    {
        var targetDir = Path.Combine(_rootPath, subDirectory);
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        var filePath = Path.Combine(targetDir, fileName);
        
        using (var destination = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destination);
        }

        return $"/{subDirectory}/{fileName}".Replace("\\", "/");
    }

    public bool Delete(string path)
    {
        var filePath = Path.Combine(_rootPath, path.TrimStart('/').Replace("/", "\\"));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }
}