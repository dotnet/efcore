// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class ManyToManyContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<EntityOne> EntityOnes { get; set; } = null!;
    public DbSet<EntityTwo> EntityTwos { get; set; } = null!;
    public DbSet<EntityThree> EntityThrees { get; set; } = null!;
    public DbSet<EntityCompositeKey> EntityCompositeKeys { get; set; } = null!;
    public DbSet<EntityRoot> EntityRoots { get; set; } = null!;
    public DbSet<ImplicitManyToManyA> ImplicitManyToManyAs { get; set; } = null!;
    public DbSet<ImplicitManyToManyB> ImplicitManyToManyBs { get; set; } = null!;
    public DbSet<GeneratedKeysLeft> GeneratedKeysLefts { get; set; } = null!;
    public DbSet<GeneratedKeysRight> GeneratedKeysRights { get; set; } = null!;
}

public static class ManyToManyContextExtensions
{
    public static TEntity CreateInstance<TEntity>(this DbSet<TEntity> set, Action<TEntity, bool>? configureEntity = null)
        where TEntity : class, new()
    {
        var isProxy = set.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>()?.UseChangeTrackingProxies == true;

        var entity = isProxy ? set.CreateProxy() : new TEntity();

        configureEntity?.Invoke(entity, isProxy);

        return entity;
    }
}
