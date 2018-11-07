// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ComputedColumnTest : IDisposable
    {
        [Fact]
        public void Can_use_computed_columns()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new Context(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreatedResiliently();

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
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new Context(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreatedResiliently();

                var entity = context.Add(new Entity { P1 = 20, P2 = 30 }).Entity;

                context.SaveChanges();

                Assert.Equal(50, entity.P4);
                Assert.Null(entity.P5);
            }
        }

        private class Context : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _databaseName;

            public Context(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
            }

            public DbSet<Entity> Entities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Entity>()
                    .Property(e => e.P4)
                    .HasComputedColumnSql("P1 + P2");

                modelBuilder.Entity<Entity>()
                    .Property(e => e.P5)
                    .HasComputedColumnSql("P1 + P3");
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

        [Flags]
        public enum FlagEnum
        {
            None = 0x0,
            AValue = 0x1,
            BValue = 0x2
        }

        public class EnumItem
        {
            public int EnumItemId { get; set; }
            public FlagEnum FlagEnum { get; set; }
            public FlagEnum? OptionalFlagEnum { get; set; }
            public FlagEnum? CalculatedFlagEnum { get; set; }
        }

        private class NullableContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly string _databaseName;

            public NullableContext(IServiceProvider serviceProvider, string databaseName)
            {
                _serviceProvider = serviceProvider;
                _databaseName = databaseName;
            }

            public DbSet<EnumItem> EnumItems { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(_databaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<EnumItem>()
                    .Property(entity => entity.CalculatedFlagEnum)
                    .HasComputedColumnSql("FlagEnum | OptionalFlagEnum");
        }

        [Fact]
        public void Can_use_computed_columns_with_nullable_enum()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            using (var context = new NullableContext(serviceProvider, TestStore.Name))
            {
                context.Database.EnsureCreatedResiliently();

                var entity = context.EnumItems.Add(new EnumItem { FlagEnum = FlagEnum.AValue, OptionalFlagEnum = FlagEnum.BValue }).Entity;
                context.SaveChanges();

                Assert.Equal(FlagEnum.AValue | FlagEnum.BValue, entity.CalculatedFlagEnum);
            }
        }

        public ComputedColumnTest()
        {
            TestStore = SqlServerTestStore.CreateInitialized("ComputedColumnTest");
        }

        protected SqlServerTestStore TestStore { get; }

        public virtual void Dispose() => TestStore.Dispose();
    }
}
