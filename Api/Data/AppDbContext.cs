using Microsoft.EntityFrameworkCore;
using Api.Entities;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<TestItem> TestItems => Set<TestItem>();
}
