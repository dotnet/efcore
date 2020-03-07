// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerConfigPatternsTest
    {
        public class ImplicitServicesAndConfig
        {
            [ConditionalFact]
            public async Task Can_query_with_implicit_services_and_OnConfiguring()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext())
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder
                        .EnableServiceProviderCaching(false)
                        .UseSqlServer(
                            SqlServerNorthwindTestStoreFactory.NorthwindConnectionString,
                            b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ImplicitServicesExplicitConfig
        {
            [ConditionalFact]
            public async Task Can_query_with_implicit_services_and_explicit_config()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext(
                        new DbContextOptionsBuilder()
                            .EnableServiceProviderCaching(false)
                            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                            .Options))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ExplicitServicesImplicitConfig
        {
            [ConditionalFact]
            public async Task Can_query_with_explicit_services_and_OnConfiguring()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext(
                        new DbContextOptionsBuilder().UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkSqlServer()
                                .BuildServiceProvider()).Options))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.UseSqlServer(
                        SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ExplicitServicesAndConfig
        {
            [ConditionalFact]
            public async Task Can_query_with_explicit_services_and_explicit_config()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext(
                        new DbContextOptionsBuilder()
                            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                            .UseInternalServiceProvider(
                                new ServiceCollection()
                                    .AddEntityFrameworkSqlServer()
                                    .BuildServiceProvider()).Options))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ExplicitServicesAndNoConfig
        {
            [ConditionalFact]
            public void Throws_on_attempt_to_use_SQL_Server_without_providing_connection_string()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            {
                                using (var context = new NorthwindContext(
                                    new DbContextOptionsBuilder().UseInternalServiceProvider(
                                        new ServiceCollection()
                                            .AddEntityFrameworkSqlServer()
                                            .BuildServiceProvider()).Options))
                                {
                                    Assert.Equal(91, context.Customers.Count());
                                }
                            }).Message);
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class NoServicesAndNoConfig
        {
            [ConditionalFact]
            public void Throws_on_attempt_to_use_context_with_no_store()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            {
                                using (var context = new NorthwindContext())
                                {
                                    Assert.Equal(91, context.Customers.Count());
                                }
                            }).Message);
                }
            }

            private class NorthwindContext : DbContext
            {
                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.EnableServiceProviderCaching(false);
            }
        }

        public class ImplicitConfigButNoServices
        {
            [ConditionalFact]
            public void Throws_on_attempt_to_use_store_with_no_store_services()
            {
                var serviceCollection = new ServiceCollection();
                new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
                var serviceProvider = serviceCollection.BuildServiceProvider();

                using (SqlServerTestStore.GetNorthwindStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
                        Assert.Throws<InvalidOperationException>(
                            () =>
                            {
                                using (var context = new NorthwindContext(
                                    new DbContextOptionsBuilder()
                                        .UseInternalServiceProvider(serviceProvider).Options))
                                {
                                    Assert.Equal(91, context.Customers.Count());
                                }
                            }).Message);
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
                    optionsBuilder.UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class InjectContext
        {
            [ConditionalFact]
            public async Task Can_register_context_with_DI_container_and_have_it_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddTransient<NorthwindContext>()
                    .AddTransient<MyController>()
                    .AddSingleton(p => new DbContextOptionsBuilder().UseInternalServiceProvider(p).Options)
                    .BuildServiceProvider();

                using (SqlServerTestStore.GetNorthwindStore())
                {
                    await serviceProvider.GetRequiredService<MyController>().TestAsync();
                }
            }

            private class MyController
            {
                private readonly NorthwindContext _context;

                public MyController(NorthwindContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public async Task TestAsync()
                    => Assert.Equal(91, await _context.Customers.CountAsync());
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                    Assert.NotNull(options);
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder.UseSqlServer(
                        SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class InjectContextAndConfiguration
        {
            [ConditionalFact]
            public async Task Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddTransient<MyController>()
                    .AddTransient<NorthwindContext>()
                    .AddSingleton(
                        new DbContextOptionsBuilder()
                            .EnableServiceProviderCaching(false)
                            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                            .Options).BuildServiceProvider();

                using (SqlServerTestStore.GetNorthwindStore())
                {
                    await serviceProvider.GetRequiredService<MyController>().TestAsync();
                }
            }

            private class MyController
            {
                private readonly NorthwindContext _context;

                public MyController(NorthwindContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public async Task TestAsync()
                    => Assert.Equal(91, await _context.Customers.CountAsync());
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                    Assert.NotNull(options);
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ConstructorArgsToBuilder
        {
            [ConditionalFact]
            public async Task Can_pass_context_options_to_constructor_and_use_in_builder()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext(
                        new DbContextOptionsBuilder()
                            .EnableServiceProviderCaching(false)
                            .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                            .Options))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(DbContextOptions options)
                    : base(options)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ConstructorArgsToOnConfiguring
        {
            [ConditionalFact]
            public async Task Can_pass_connection_string_to_constructor_and_use_in_OnConfiguring()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    using (var context = new NorthwindContext(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                private readonly string _connectionString;

                public NorthwindContext(string connectionString)
                {
                    _connectionString = connectionString;
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder
                        .EnableServiceProviderCaching(false)
                        .UseSqlServer(_connectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class NestedContext
        {
            [ConditionalFact]
            public async Task Can_use_one_context_nested_inside_another_of_the_same_type()
            {
                using (SqlServerTestStore.GetNorthwindStore())
                {
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .BuildServiceProvider();

                    using (var context1 = new NorthwindContext(serviceProvider))
                    {
                        var customers1 = await context1.Customers.ToListAsync();
                        Assert.Equal(91, customers1.Count);
                        Assert.Equal(91, context1.ChangeTracker.Entries().Count());

                        using (var context2 = new NorthwindContext(serviceProvider))
                        {
                            Assert.Empty(context2.ChangeTracker.Entries());

                            var customers2 = await context2.Customers.ToListAsync();
                            Assert.Equal(91, customers2.Count);
                            Assert.Equal(91, context2.ChangeTracker.Entries().Count());

                            Assert.Equal(customers1[0].CustomerID, customers2[0].CustomerID);
                            Assert.NotSame(customers1[0], customers2[0]);
                        }
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                private readonly IServiceProvider _serviceProvider;

                public NorthwindContext(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Customer
        {
            public string CustomerID { get; set; }

            // ReSharper disable UnusedMember.Local
            public string CompanyName { get; set; }

            public string Fax { get; set; }
            // ReSharper restore UnusedMember.Local
        }

        private static void ConfigureModel(ModelBuilder builder)
            => builder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.CustomerID);
                    b.ToTable("Customers");
                });
    }
}
