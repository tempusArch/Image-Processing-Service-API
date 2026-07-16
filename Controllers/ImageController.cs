using ImageProcessingServiceApi.Domain;
using ImageProcessingServiceApi.Infrastructure;
using ImageProcessingServiceApi.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SixLabors.ImageSharp.Processing;
using StackExchange.Redis;
using ImageProcessingServiceAPI.Domain;


namespace ImageProcessingServiceApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase {
    private readonly ImageService _imageService;
    private readonly LocalStorageService _localStorageService;
    private readonly ImageProcessingServiceApiDbContext _context;
    private readonly RabbitMqPublisher _publisher;
    private readonly IDatabase _redisDB;
    public ImageController(ImageService imageService, LocalStorageService localStorageService, ImageProcessingServiceApiDbContext context, RabbitMqPublisher publisher, IConnectionMultiplexer redisConnectionMultiplexer) {
        _imageService = imageService;
        _localStorageService = localStorageService;
        _context = context;
        _publisher = publisher;
        _redisDB = redisConnectionMultiplexer.GetDatabase();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        if (file == null || file.Length == 0)
            return BadRequest("The uploaded file is empty");

        if (file.Length > (15 * 1024 * 1024))
            return BadRequest("the uploaded file exceeds 15 MB size limit");

        using var stream = file.OpenReadStream();
        var validated = await _imageService.ValidateUpload(stream, cancellationToken);

        string relativePath = @$"{userId}/{validated}";
        await _localStorageService.UploadToLocalStorage(relativePath, stream, cancellationToken);

        return Created(string.Empty, validated);

    }

    [HttpGet("{resultName}")]
    public async Task<IActionResult> GetCompletedImage(string resultName, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        var theOne = await _context.JobTable
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.ResultName == resultName, cancellationToken);
        
        if (theOne == null)
            return Accepted(new { resultName, Status = "Working" });

        string relativePath = @$"Completed/{resultName}";
        var content = await _localStorageService.ReadLocalStroage(relativePath);

        return File(content, _imageService.GetContentType(Path.GetExtension(resultName)));
    }

    [HttpPost("resize")]
    public async Task<IActionResult> ResizeImage(string imageId, int width, int height, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"resize-{width}-{height}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new ResizeMessage {
            UserId = userId,
            ImageId = imageId,
            Width = width,
            Height = height,
            ResultName = resultName
        };
        
        await _publisher.ResizePublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }
    
    [HttpPost("crop")]
    public async Task<IActionResult> CropImage(string imageId, int x, int y, int width, int height, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"crop-{width}-{height}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new CropMessage {
            UserId = userId,
            ImageId = imageId,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            ResultName = resultName
        };
        
        await _publisher.CropPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }
    
    [HttpPost("rotate")]
    public async Task<IActionResult> RotateImage(string imageId, float degree, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"rotate-{degree}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new RotateMessage {
            UserId = userId,
            ImageId = imageId,
            Degree = degree,
            ResultName = resultName
        };
        
        await _publisher.RotatePublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("watermark")]
    public async Task<IActionResult> AddWatermark(string imageId, string watermarkText, int x, int y, float size, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"watermark-{x}-{y}-{size}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new WatermarkMessage {
            UserId = userId,
            ImageId = imageId,
            WatermarkText = watermarkText,
            X = x,
            Y = y,
            Size = size,
            ResultName = resultName
        };
        
        await _publisher.WatermarkPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("flip")]
    public async Task<IActionResult> FlipImage(string imageId, FlipMode flipMode, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"flip-{flipMode}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new FlipMessage {
            UserId = userId,
            ImageId = imageId,
            flipMode = flipMode,
            ResultName = resultName
        };
        
        await _publisher.FlipPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("mirror")]
    public async Task<IActionResult> MirrorImage(string imageId, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"mirror-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new MirrorMessage {
            UserId = userId,
            ImageId = imageId,
            ResultName = resultName
        };
        
        await _publisher.MirrorPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("compress")]
    public async Task<IActionResult> CompressImage(string imageId, CancellationToken cancellationToken, int quality = 75) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        if (quality < 1 || quality > 100)
            return BadRequest("Quality should be between 1 and 100");

        string resultName = $"compress-{quality}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new CompressMessage {
            UserId = userId,
            ImageId = imageId,
            Quality = quality,
            ResultName = resultName
        };
        
        await _publisher.CompressPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("changeFormat")]
    public async Task<IActionResult> ChangeFormat(string imageId, TargetFormat targetFormat, CancellationToken cancellationToken, int quality = 75) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        if (quality < 1 || quality > 100)
            return BadRequest("Quality should be between 1 and 100");

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageId);
        string resultName = $"changeFormat-{fileNameWithoutExtension}.{targetFormat}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new ChangeFormatMessage {
            UserId = userId,
            ImageId = imageId,
            targetFormat = targetFormat,
            Quality = quality,
            ResultName = resultName
        };
        
        await _publisher.ChangeFormatPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }

    [HttpPost("filter")]
    public async Task<IActionResult> ApplyFilter(string imageId, ImageFilter imageFilter, CancellationToken cancellationToken) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID claim is missing");

        string resultName = $"filter-{imageFilter}-{imageId}";

        if (_redisDB.KeyExists(resultName)) {
            var cachedImage = await _redisDB.StringGetAsync(resultName);
            return File((byte[])cachedImage!, _imageService.GetContentType(Path.GetExtension(resultName)));
        }
        
        var message = new FilterMessage {
            UserId = userId,
            ImageId = imageId,
            imageFilter = imageFilter,
            ResultName = resultName
        };
        
        await _publisher.FilterPublishAsync(message);

        return Accepted(new { resultName, Status = "Working" });
    }
}