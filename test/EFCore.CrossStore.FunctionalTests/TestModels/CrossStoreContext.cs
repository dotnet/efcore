// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels
{
    public class CrossStoreContext : DbContext
    {
        public CrossStoreContext()
        {
        }

        public CrossStoreContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<SimpleEntity> SimpleEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleEntity>(
                eb =>
                {
                    eb.ToTable("RelationalSimpleEntity");
                    eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                    eb.HasKey(e => e.Id);
                    eb.Property(e => e.Id).UseIdentityColumn();
                });
        }

        public static void RemoveAllEntities(CrossStoreContext context)
            => context.SimpleEntities.RemoveRange(context.SimpleEntities);
    }
}
