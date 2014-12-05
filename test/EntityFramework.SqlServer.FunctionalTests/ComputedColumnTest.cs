// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class ComputedColumnTest
    {
        [Fact]
        public void Can_use_computed_columns()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new Context(serviceProvider, "ComputedColumns"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var entity = context.Add(new Entity { P1 = 20, P2 = 30, P3 = 80 }).Entity;

                context.SaveChanges();

                Assert.Equal(50, entity.P4);
                Assert.Equal(100, entity.P5);
            }
        }

        [Fact]
        public void Can_use_computed_columns_with_null_values()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();

            using (var context = new Context(serviceProvider, "ComputedColumns"))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var entity = context.Add(new Entity { P1 = 20, P2 = 30 }).Entity;

                context.SaveChanges();

                Assert.Equal(50, entity.P4);
                Assert.Null(entity.P5);
            }
        }

        private class Context : DbContext
        {
            private readonly string _databaseName;

            public Context(IServiceProvider serviceProvider, string databaseName)
                : base(serviceProvider)
            {
                _databaseName = databaseName;
            }

            public DbSet<Entity> Entities { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity>()
                    .Property(e => e.P4)
                    .StoreComputed()
                    .ForSqlServer().DefaultExpression("P1 + P2");

                modelBuilder.Entity<Entity>()
                    .Property(e => e.P5)
                    .StoreComputed()
                    .ForSqlServer().DefaultExpression("P1 + P3");
            }
        }

        private class Entity
        {
            public int Id { get; set; }
            public int P1 { get; set; }
            public int P2 { get; set; }
            public int? P3 { get; set; }
            public int P4 { get; set; }
            public int? P5 { get; set; }
        }
    }
}
