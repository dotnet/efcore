// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class ManyToManyContext : PoolableDbContext
    {
        public ManyToManyContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<EntityOne> EntityOnes { get; set; }
        public DbSet<EntityTwo> EntityTwos { get; set; }
        public DbSet<EntityThree> EntityThrees { get; set; }
        public DbSet<EntityCompositeKey> EntityCompositeKeys { get; set; }
        public DbSet<EntityRoot> EntityRoots { get; set; }
        public DbSet<ImplicitManyToManyA> ImplicitManyToManyAs { get; set; }
        public DbSet<ImplicitManyToManyB> ImplicitManyToManyBs { get; set; }
        public DbSet<GeneratedKeysLeft> GeneratedKeysLefts { get; set; }
        public DbSet<GeneratedKeysRight> GeneratedKeysRights { get; set; }

        public static void Seed(ManyToManyContext context)
            => ManyToManyData.Seed(context);
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
}
