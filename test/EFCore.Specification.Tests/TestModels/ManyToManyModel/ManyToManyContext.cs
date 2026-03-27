// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class ManyToManyContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<EntityOne> EntityOnes { get; set; }
    public DbSet<EntityTwo> EntityTwos { get; set; }
    public DbSet<EntityThree> EntityThrees { get; set; }
    public DbSet<EntityCompositeKey> EntityCompositeKeys { get; set; }
    public DbSet<EntityRoot> EntityRoots { get; set; }
    public DbSet<EntityTableSharing1> EntityTableSharing1s { get; set; }
    public DbSet<EntityTableSharing2> EntityTableSharing2s { get; set; }
    public DbSet<ImplicitManyToManyA> ImplicitManyToManyAs { get; set; }
    public DbSet<ImplicitManyToManyB> ImplicitManyToManyBs { get; set; }
    public DbSet<GeneratedKeysLeft> GeneratedKeysLefts { get; set; }
    public DbSet<GeneratedKeysRight> GeneratedKeysRights { get; set; }
    public DbSet<UnidirectionalEntityOne> UnidirectionalEntityOnes { get; set; }
    public DbSet<UnidirectionalEntityTwo> UnidirectionalEntityTwos { get; set; }
    public DbSet<UnidirectionalEntityThree> UnidirectionalEntityThrees { get; set; }
    public DbSet<UnidirectionalEntityCompositeKey> UnidirectionalEntityCompositeKeys { get; set; }
    public DbSet<UnidirectionalEntityRoot> UnidirectionalEntityRoots { get; set; }
    public DbSet<UnidirectionalGeneratedKeysLeft> UnidirectionalGeneratedKeysLefts { get; set; }
    public DbSet<UnidirectionalGeneratedKeysRight> UnidirectionalGeneratedKeysRights { get; set; }
}

public static class ManyToManyContextExtensions
{
    public static TEntity CreateInstance<TEntity>(this DbSet<TEntity> set, Action<TEntity, bool> configureEntity = null)
        where TEntity : class, new()
    {
        var isProxy = set.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>()?.UseChangeTrackingProxies == true;

        var entity = isProxy ? set.CreateProxy() : new TEntity();

        configureEntity?.Invoke(entity, isProxy);

        return entity;
    }
}
