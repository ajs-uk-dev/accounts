using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence;

public class PracticeOperationsDbContext : DbContext
{
    public PracticeOperationsDbContext(DbContextOptions<PracticeOperationsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("practice_operations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PracticeOperationsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
