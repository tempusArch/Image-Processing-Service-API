using System.Globalization;
using System.Security.Claims;
using ImageProcessingServiceAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessingServiceAPI.Infrastructure;

public class HttpContextService {
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpContextService(IHttpContextAccessor httpContextAccessor) {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserEmail() {
        return _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;
    }

    public void CheckUserIdClaim() {
        var userId =_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID claim is missing");
    }

    public int GetCurrentUserId() {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null || httpContext.User == null || !httpContext.User.Claims.Any())
            return 0;

        var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("User ID claim is missing");

        return int.Parse(claim.Value);
    }

    public string? GetRefreshToken() {
        return _httpContextAccessor.HttpContext?.Request.Cookies["RefreshToken"];
    }

    public void SetRefreshToken(RefreshToken newRefreshToken) {
        _httpContextAccessor.HttpContext?.Response.Cookies
            .Append(
                "RefreshToken", 
                newRefreshToken.Token, 
                new CookieOptions {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = newRefreshToken.ExpiresAt
                }
            );
    }

    public void DeleteRefreshToken() {
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("RefreshToken");

    }
}