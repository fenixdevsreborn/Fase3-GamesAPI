using Fcg.Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fcg.Games.Infrastructure.Persistence;

public class GamesDbContext : DbContext
{
    public GamesDbContext(DbContextOptions<GamesDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserGameLibrary> UserGameLibraries => Set<UserGameLibrary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamesDbContext).Assembly);
    }
}
