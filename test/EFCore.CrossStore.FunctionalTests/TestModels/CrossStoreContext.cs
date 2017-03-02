// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.CrossStore.FunctionalTests.TestModels
{
    public class CrossStoreContext : DbContext
    {
        public CrossStoreContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<SimpleEntity> SimpleEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<SimpleEntity>(eb =>
                    {
                        eb.Property(typeof(string), SimpleEntity.ShadowPartitionIdName);
                        eb.ToTable("RelationalSimpleEntity"); // TODO: specify schema when #948 is fixed
                        eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                        eb.HasKey(e => e.Id);
                    });
        }

        public static void RemoveAllEntities(CrossStoreContext context)
            => context.SimpleEntities.RemoveRange(context.SimpleEntities);
    }
}
