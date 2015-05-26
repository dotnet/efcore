// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels
{
    public class CrossStoreContext : DbContext
    {
        public static readonly string AtsTableSuffix = Guid.NewGuid().ToString().Replace("-", "");

        public CrossStoreContext(IServiceProvider serviceProvider, EntityOptions options)
            : base(serviceProvider, options)
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
                        eb
                            .Table("RelationalSimpleEntity") // TODO: specify schema when #948 is fixed
                            .ForSqlServer(b => { eb.Property(e => e.Id).ForSqlServer().UseSequence(); });

                        eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                        eb.Key(e => e.Id);
                    });
        }

        public static void RemoveAllEntities(CrossStoreContext context)
        {
            context.SimpleEntities.RemoveRange(context.SimpleEntities);
        }
    }
}
