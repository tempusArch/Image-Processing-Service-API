using ImageProcessingServiceAPI.Domain;
using ImageProcessingServiceAPI.Infrastructure;

namespace ImageProcessingServiceAPI.Application;

public class LocalStorageService {
    private static string basePath = Directory.GetCurrentDirectory() + @"\Data";
    private readonly ImageProcessingServiceApiDbContext _context;

    public LocalStorageService(ImageProcessingServiceApiDbContext context) {
        _context = context;
    }

    public async Task UploadToLocalStorage(string relativePath, Stream content, CancellationToken cancellationToken) {
        var combined = CombinePath(relativePath);
        var directory = Path.GetDirectoryName(combined);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var output = File.Create(combined);
        content.Position = 0;
        await content.CopyToAsync(output, cancellationToken);
        await output.FlushAsync(cancellationToken);
        
    }

    public Task<Stream> ReadLocalStroage(string relativePath) {
        var combined = CombinePath(relativePath);
        Stream content = File.OpenRead(combined);
        return Task.FromResult(content);
    }

    public async Task CompletedBytesSaveToLocal(string resultName, byte[] content, CancellationToken cancellationToken) {
        var combined = CombinePath(@$"Completed\{resultName}");

        var directory = Path.GetDirectoryName(combined);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllBytesAsync(combined, content);

        var theOne = new Job {
            ResultName = resultName
        };

        _context.JobTable.Add(theOne);
        await _context.SaveChangesAsync(cancellationToken);

    }

    private string CombinePath(string relativePath) {
        basePath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var combined = Path.GetFullPath(Path.Combine(basePath, relativePath));

        if (!combined.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenException("Invalid access");

        return combined;
    }
}