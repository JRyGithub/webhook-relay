using Microsoft.EntityFrameworkCore;

namespace WebhookRelay.Data;

// Entities (events, targets, deliveries, delivery_attempts) are added from M1 onwards.
public class RelayDbContext(DbContextOptions<RelayDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelayDbContext).Assembly);
    }
}
