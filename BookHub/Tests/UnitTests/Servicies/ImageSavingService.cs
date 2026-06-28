 using System.Text;
using Microsoft.AspNetCore.Http;

namespace Tests.UnitTests.Servicies;

[TestFixture]
public class ImageSavingTests
{
    private class TestFileImageService
    {
        private readonly string _baseDir;
        public TestFileImageService(string baseDir)
        {
            _baseDir = baseDir;
            Directory.CreateDirectory(_baseDir);
        }

        public async Task<string> SaveBookImageAsync(IFormFile file, string bookName)
        {
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".img";
            var fileName = $"{bookName}_{Guid.NewGuid()}{ext}";
            var path = Path.Combine(_baseDir, fileName);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);
            return path;
        }

        public Task DeleteBookImageAsync(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task SaveBookImage_WritesFile_And_CanDeleteIt()
    {
        // arrange
        var content = "fake-image-bytes";
        var bytes = Encoding.UTF8.GetBytes(content);
        using var ms = new MemoryStream(bytes);
        ms.Position = 0;

        IFormFile formFile = new FormFile(ms, 0, ms.Length, "image", "cover.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var tmpDir = Path.Combine(Path.GetTempPath(), "bookhub_test_images", Guid.NewGuid().ToString());
        var svc = new TestFileImageService(tmpDir);

        var savedPath = await svc.SaveBookImageAsync(formFile, "MyBook");

        Assert.That(File.Exists(savedPath), Is.True, "Soubor nebyl uložen.");
        var savedBytes = await File.ReadAllBytesAsync(savedPath);
        Assert.That(savedBytes, Is.EqualTo(bytes));

        await svc.DeleteBookImageAsync(savedPath);
        Assert.That(File.Exists(savedPath), Is.False);

        if (Directory.Exists(tmpDir))
            Directory.Delete(tmpDir, true);
    }
}