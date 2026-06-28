namespace DataAccessLayer.Repository.Image;

public interface IImageRepository
{
    Task<string> UploadAsync(Stream fileStream, string subDirectory, string fileName);
    bool Delete(string path);
}