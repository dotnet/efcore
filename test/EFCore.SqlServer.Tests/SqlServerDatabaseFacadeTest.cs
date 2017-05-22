// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public class SqlServerDatabaseFacadeTest
    {
        [Fact]
        public void IsSqlServer_when_using_OnConfguring()
        {
            using (var context = new SqlServerOnConfiguringContext())
            {
                Assert.True(context.Database.IsSqlServer());
            }
        }

        [Fact]
        public void IsSqlServer_in_OnModelCreating_when_using_OnConfguring()
        {
            using (new SqlServerOnModelContext())
            {
            }
        }

        [Fact]
        public void IsSqlServer_in_constructor_when_using_OnConfguring()
        {
            using (new SqlServerConstructorContext())
            {
            }
        }

        [Fact]
        public void Cannot_use_IsSqlServer_in_OnConfguring()
        {
            using (var context = new SqlServerUseInOnConfiguringContext())
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            {
                                var _ = context.Model; // Trigger context initialization
                            }).Message);
            }
        }

        [Fact]
        public void IsSqlServer_when_using_constructor()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseSqlServer("Database=Maltesers").Options))
            {
                Assert.True(context.Database.IsSqlServer());
            }
        }

        [Fact]
        public void IsSqlServer_in_OnModelCreating_when_using_constructor()
        {
            using (var context = new ProviderOnModelContext(
                new DbContextOptionsBuilder().UseSqlServer("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsSqlServerSet);
            }
        }

        [Fact]
        public void IsSqlServer_in_constructor_when_using_constructor()
        {
            using (var context = new ProviderConstructorContext(
                new DbContextOptionsBuilder().UseSqlServer("Database=Maltesers").Options))
            {
                var _ = context.Model; // Trigger context initialization
                Assert.True(context.IsSqlServerSet);
            }
        }

        [Fact]
        public void Cannot_use_IsSqlServer_in_OnConfguring_with_constructor()
        {
            using (var context = new ProviderUseInOnConfiguringContext(
                new DbContextOptionsBuilder().UseSqlServer("Database=Maltesers").Options))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            {
                                var _ = context.Model; // Trigger context initialization
                            }).Message);
            }
        }

        [Fact]
        public void Not_IsSqlServer_when_using_different_provider()
        {
            using (var context = new ProviderContext(
                new DbContextOptionsBuilder().UseInMemoryDatabase("Maltesers").Options))
            {
                Assert.False(context.Database.IsSqlServer());
            }
        }

        private class ProviderContext : DbContext
        {
            protected ProviderContext()
            {
            }

            public ProviderContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class SqlServerOnConfiguringContext : ProviderContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer("Database=Maltesers");
        }

        private class SqlServerOnModelContext : SqlServerOnConfiguringContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => Assert.True(Database.IsSqlServer());
        }

        private class SqlServerConstructorContext : SqlServerOnConfiguringContext
        {
            public SqlServerConstructorContext()
                => Assert.True(Database.IsSqlServer());
        }

        private class SqlServerUseInOnConfiguringContext : SqlServerOnConfiguringContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                Database.IsSqlServer();
            }
        }

        private class ProviderOnModelContext : ProviderContext
        {
            public ProviderOnModelContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsSqlServerSet = Database.IsSqlServer();

            public bool? IsSqlServerSet { get; private set; }
        }

        private class ProviderConstructorContext : ProviderContext
        {
            public ProviderConstructorContext(DbContextOptions options)
                : base(options)
                => IsSqlServerSet = Database.IsSqlServer();

            public bool? IsSqlServerSet { get; }
        }

        private class ProviderUseInOnConfiguringContext : ProviderContext
        {
            public ProviderUseInOnConfiguringContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => Database.IsSqlServer();
        }
    }
}
