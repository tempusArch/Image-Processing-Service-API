using System.ComponentModel.DataAnnotations;
using ImageProcessingServiceAPI.Domain;
using ImageProcessingServiceAPI.Infrastructure;
using StackExchange.Redis;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using Microsoft.Extensions.WebEncoders;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ImageProcessingServiceAPI.Application;

public class ImageService {
    private const long maxSize = 15 * 1024 * 1024;
    private static readonly HashSet<string> SupportedFormats = new (StringComparer.OrdinalIgnoreCase) {
        "jfif",
        "jpeg",
        "jpg",
        "png",
        "webp"
    };

    private readonly IDatabase _redisDB;
    private readonly LocalStorageService _localStorage;

    public ImageService(IConnectionMultiplexer redisConnectionMultiplexer, LocalStorageService localStorageService) {
        _redisDB = redisConnectionMultiplexer.GetDatabase();
        _localStorage = localStorageService;
    }

    public async Task<string> ValidateUpload(Stream stream, CancellationToken cancellationToken) {     
        var info = await Image.IdentifyAsync(stream, cancellationToken);

        if (info == null || info.Metadata.DecodedImageFormat == null)
            throw new ValidationException("The uploaded file is not a supported image");

        string format = info.Metadata.DecodedImageFormat.Name.ToLowerInvariant();

        if (!SupportedFormats.Contains(format))
            throw new ValidationException("Only JPEG, JPG, JFIF, PNG and WebP are supported");

        stream.Position = 0;
        var imageId = Guid.NewGuid().ToString();
        var extension = GetExtension(format);
        return $"{imageId}{extension}";

    }

    public async Task<(string, byte[])> ResizeImage(string userId, string imageId, int width, int height, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        image.Mutate(x => x.Resize(
            new ResizeOptions {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop
            }
        ));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> CropImage(string userId, string imageId, int x, int y, int width, int height, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        image.Mutate(m => m.Crop(
            new Rectangle(x, y, width, height)
        ));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> RotateImage(string userId, string imageId, float degree, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        image.Mutate(x => x.Rotate(degree));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> AddWatermark(string userId, string imageId, string watermarkText, int x, int y, float size, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();
        var font = SystemFonts.CreateFont("Arial", size, FontStyle.Regular);

        image.Mutate(m => m.Paint(canvas => {
            canvas.DrawText(
                new RichTextOptions(font) {
                    Origin = new PointF(x, y)
                },
                watermarkText,
                Brushes.Solid(Color.White),
                Pens.Solid(Color.Black, 2)
            );
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> FlipImage(string userId, string imageId, FlipMode flipMode, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        image.Mutate(x => x.Flip(flipMode));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> MirrorImage(string userId, string imageId, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        image.Mutate(x => x.Flip(FlipMode.Horizontal));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> CompressImage(string userId, string imageId, int quality, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format, quality), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    public async Task<(string, byte[])> ChangeFormat(string userId, string imageId, TargetFormat targetFormat, int quality, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string sourceFormat = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        if (sourceFormat == targetFormat.ToString())       
            throw new ValidationException("Target format is same with original one");

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(targetFormat.ToString(), quality), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    } 

    public async Task<(string, byte[])> ApplyFilter(string userId, string imageId, ImageFilter imageFilter, string resultName, CancellationToken cancellationToken) {
        using var imageStream = await _localStorage.ReadLocalStroage(@$"{userId}\{imageId}");
        using var image = await Image.LoadAsync(imageStream, cancellationToken);

        string format = image.Metadata.DecodedImageFormat!.Name.ToLowerInvariant();

        switch(imageFilter) {
            case ImageFilter.GrayScale:
                image.Mutate(x => x.Grayscale());
                break;

            case ImageFilter.Sepia:
                image.Mutate(x => x.Sepia());
                break;

            default:
                break;
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, GetEncoder(format), cancellationToken);
        var content = outputStream.ToArray();

        await _redisDB.StringSetAsync(resultName, content, TimeSpan.FromHours(1));

        return (resultName, content);
    }

    #region helper methods
    private static string FormatToString(IImageFormat format) {
        var name = format.Name.ToLowerInvariant();

        switch (name) {
            case "jpg":
                return "jpeg";

            case "jfif":
                return "jpeg";

            default:
                return name;

        }
    }

    private static string GetExtension(string format) {
        switch (format) {
            case "png":
                return ".png";

            case "webp":
                return ".webp";

            default:
                return ".jpeg";
        }
    }

    public string GetContentType(string format) {
        switch (format) {
            case "png":
                return "image/png";

            case "webp":
                return "image/webp";

            default:
                return "image/jpeg";
        }
    }

    private static IImageEncoder GetEncoder(string format, int quality = 75) {
        switch (format) {
            case "png":
                return new PngEncoder();

            case "webp":
                return new WebpEncoder() {Quality = quality};

            default:
                return new JpegEncoder() {Quality = quality};
        }
    }
    #endregion
}