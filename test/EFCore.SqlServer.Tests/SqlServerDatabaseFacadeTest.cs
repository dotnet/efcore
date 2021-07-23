// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerDatabaseFacadeTest
    {
        [ConditionalFact]
        public void IsSqlServer_when_using_OnConfiguring()
        {
            using var context = new SqlServerOnConfiguringContext();
            Assert.True(context.Database.IsSqlServer());
        }

        [ConditionalFact]
        public void IsSqlServer_in_OnModelCreating_when_using_OnConfiguring()
        {
            using var context = new SqlServerOnModelContext();
            var _ = context.Model; // Trigger context initialization
            Assert.True(context.IsSqlServerSet);
        }

        [ConditionalFact]
        public void IsRelational_in_OnModelCreating_when_using_OnConfiguring()
        {
            using var context = new RelationalOnModelContext();
            var _ = context.Model; // Trigger context initialization
            Assert.True(context.IsSqlServerSet);
        }

        [ConditionalFact]
        public void IsSqlServer_in_constructor_when_using_OnConfiguring()
        {
            using var context = new SqlServerConstructorContext();
            var _ = context.Model; // Trigger context initialization
            Assert.True(context.IsSqlServerSet);
        }

        [ConditionalFact]
        public void Cannot_use_IsSqlServer_in_OnConfiguring()
        {
            using var context = new SqlServerUseInOnConfiguringContext();
            Assert.Equal(
                CoreStrings.RecursiveOnConfiguring,
                Assert.Throws<InvalidOperationException>(
                    () =>
                    {
                        var _ = context.Model; // Trigger context initialization
                    }).Message);
        }

        [ConditionalFact]
        public void IsSqlServer_when_using_constructor()
        {
            using var context = new ProviderContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                    .UseSqlServer("Database=Maltesers").Options);
            Assert.True(context.Database.IsSqlServer());
        }

        [ConditionalFact]
        public void IsSqlServer_in_OnModelCreating_when_using_constructor()
        {
            using var context = new ProviderOnModelContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                    .UseSqlServer("Database=Maltesers").Options);
            var _ = context.Model; // Trigger context initialization
            Assert.True(context.IsSqlServerSet);
        }

        [ConditionalFact]
        public void IsSqlServer_in_constructor_when_using_constructor()
        {
            using var context = new ProviderConstructorContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                    .UseSqlServer("Database=Maltesers").Options);
            var _ = context.Model; // Trigger context initialization
            Assert.True(context.IsSqlServerSet);
        }

        [ConditionalFact]
        public void Cannot_use_IsSqlServer_in_OnConfiguring_with_constructor()
        {
            using var context = new ProviderUseInOnConfiguringContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                    .UseSqlServer("Database=Maltesers").Options);
            Assert.Equal(
                CoreStrings.RecursiveOnConfiguring,
                Assert.Throws<InvalidOperationException>(
                    () =>
                    {
                        var _ = context.Model; // Trigger context initialization
                    }).Message);
        }

        [ConditionalFact]
        public void Not_IsSqlServer_when_using_different_provider()
        {
            using var context = new ProviderContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                    .UseInMemoryDatabase("Maltesers").Options);
            Assert.False(context.Database.IsSqlServer());
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

            public bool? IsSqlServerSet { get; protected set; }
        }

        private class SqlServerOnConfiguringContext : ProviderContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(SqlServerFixture.DefaultServiceProvider)
                    .UseSqlServer("Database=Maltesers");
        }

        private class SqlServerOnModelContext : SqlServerOnConfiguringContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsSqlServerSet = Database.IsSqlServer();
        }

        private class RelationalOnModelContext : SqlServerOnConfiguringContext
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => IsSqlServerSet = Database.IsRelational();
        }

        private class SqlServerConstructorContext : SqlServerOnConfiguringContext
        {
            public SqlServerConstructorContext()
                => IsSqlServerSet = Database.IsSqlServer();
        }

        private class SqlServerUseInOnConfiguringContext : SqlServerOnConfiguringContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);

                IsSqlServerSet = Database.IsSqlServer();
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
        }

        private class ProviderConstructorContext : ProviderContext
        {
            public ProviderConstructorContext(DbContextOptions options)
                : base(options)
                => IsSqlServerSet = Database.IsSqlServer();
        }

        private class ProviderUseInOnConfiguringContext : ProviderContext
        {
            public ProviderUseInOnConfiguringContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => IsSqlServerSet = Database.IsSqlServer();
        }
    }
}
