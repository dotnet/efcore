using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SetClient;

public sealed class Widget
{
    public int Id { get; set; }
}

public sealed class CachedSet(Widget entity) : DbSet<Widget>
{
    public override IEntityType EntityType
        => throw new NotSupportedException();

    public override ValueTask<Widget?> FindAsync(object?[]? keyValues, CancellationToken cancellationToken)
        => new(entity);
}

public static class Client
{
    public static ValueTask<Widget?> ReadAsync(DbSet<Widget> set, int id, CancellationToken cancellationToken)
        => set.FindAsync([id], cancellationToken);
}
