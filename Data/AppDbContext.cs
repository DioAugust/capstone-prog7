using Microsoft.EntityFrameworkCore;
using AudioUploadApi.Entities;

namespace AudioUploadApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<AudioFile> AudioFiles { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}