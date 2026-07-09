using ImageProcessingServiceApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessingServiceApi.Infrastructure;

public class ImageProcessingServiceApiDbContext : DbContext {
    public ImageProcessingServiceApiDbContext(DbContextOptions<ImageProcessingServiceApiDbContext> options) : base(options) {

    }
    public DbSet<User> UserTable {get; set;}
    public DbSet<RefreshToken> RefreshTokenTable {get; set;}   
    public DbSet<Job> JobTable {get; set;}
}

    
