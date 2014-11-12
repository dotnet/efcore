// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels
{
    public class CrossStoreContext : DbContext
    {
        public static readonly string AtsTableSuffix = Guid.NewGuid().ToString().Replace("-", "");

        public CrossStoreContext(IServiceProvider serviceProvider, DbContextOptions options)
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
                            .ForRelational(b => b.Table("RelationalSimpleEntity")) // TODO: specify schema when #948 is fixed
                            .ForAzureTableStorage(b =>
                                {
                                    b.Table("ATSSimpleEntity" + AtsTableSuffix);
                                    b.PartitionAndRowKey(SimpleEntity.ShadowPartitionIdName, "Id");
                                })
                            .ForSqlServer(b =>
                                {
                                    b.Table("SqlServerSimpleEntity"); // TODO: specify schema when #948 is fixed
                                    eb.Property(e => e.Id).ForSqlServer().UseSequence();
                                });

                        eb.Property(typeof(string), SimpleEntity.ShadowPropertyName);
                        eb.Property(e => e.Id).GenerateValueOnAdd(false);
                        eb.Key(e => e.Id);
                    });
        }

        public static void RemoveAllEntities(CrossStoreContext context)
        {
            context.SimpleEntities.RemoveRange(context.SimpleEntities);
        }
    }
}
