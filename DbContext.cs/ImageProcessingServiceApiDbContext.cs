using ImageProcessingServiceAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessingServiceAPI.Infrastructure;

public class ImageProcessingServiceApiDbContext : DbContext {
    public ImageProcessingServiceApiDbContext(DbContextOptions<ImageProcessingServiceApiDbContext> options) : base(options) {

    }
    public DbSet<User> UserTable {get; set;}
    public DbSet<RefreshToken> RefreshTokenTable {get; set;}   
    public DbSet<Job> JobTable {get; set;}
}

    
