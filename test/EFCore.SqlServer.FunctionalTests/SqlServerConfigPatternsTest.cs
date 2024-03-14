// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Local

#pragma warning disable RCS1102 // Make class static.
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerConfigPatternsTest
{
    public class ImplicitServicesAndConfig
    {
        [ConditionalFact]
        public async Task Can_query_with_implicit_services_and_OnConfiguring()
        {
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext();
                Assert.Equal(91, await context.Customers.CountAsync());
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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                        .Options);
                Assert.Equal(91, await context.Customers.CountAsync());
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext(
                    new DbContextOptionsBuilder().UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkSqlServer()
                            .BuildServiceProvider(validateScopes: true)).Options);
                Assert.Equal(91, await context.Customers.CountAsync());
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext(
                    new DbContextOptionsBuilder()
                        .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkSqlServer()
                                .BuildServiceProvider(validateScopes: true)).Options);
                Assert.Equal(91, await context.Customers.CountAsync());
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => ConfigureModel(modelBuilder);
        }
    }

    public class ExplicitServicesAndNoConfig
    {
        [ConditionalFact]
        public async Task Throws_on_attempt_to_use_SQL_Server_without_providing_connection_string()
        {
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        {
                            using var context = new NorthwindContext(
                                new DbContextOptionsBuilder().UseInternalServiceProvider(
                                    new ServiceCollection()
                                        .AddEntityFrameworkSqlServer()
                                        .BuildServiceProvider(validateScopes: true)).Options);
                            Assert.Equal(91, context.Customers.Count());
                        }).Message);
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => ConfigureModel(modelBuilder);
        }
    }

    public class NoServicesAndNoConfig
    {
        [ConditionalFact]
        public async Task Throws_on_attempt_to_use_context_with_no_store()
        {
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        {
                            using var context = new NorthwindContext();
                            Assert.Equal(91, context.Customers.Count());
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
        public async Task Throws_on_attempt_to_use_store_with_no_store_services()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(
                        () =>
                        {
                            using var context = new NorthwindContext(
                                new DbContextOptionsBuilder()
                                    .UseInternalServiceProvider(serviceProvider).Options);
                            Assert.Equal(91, context.Customers.Count());
                        }).Message);
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(
                    SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

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
                .BuildServiceProvider(validateScopes: true);

            using (await SqlServerTestStore.GetNorthwindStoreAsync())
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
                        .Options).BuildServiceProvider(validateScopes: true);

            using (await SqlServerTestStore.GetNorthwindStoreAsync())
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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext(
                    new DbContextOptionsBuilder()
                        .EnableServiceProviderCaching(false)
                        .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration())
                        .Options);
                Assert.Equal(91, await context.Customers.CountAsync());
            }
        }

        private class NorthwindContext(DbContextOptions options) : DbContext(options)
        {
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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                using var context = new NorthwindContext(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString);
                Assert.Equal(91, await context.Customers.CountAsync());
            }
        }

        private class NorthwindContext(string connectionString) : DbContext
        {
            private readonly string _connectionString = connectionString;

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
            using (await SqlServerTestStore.GetNorthwindStoreAsync())
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .BuildServiceProvider(validateScopes: true);

                using var context1 = new NorthwindContext(serviceProvider);
                var customers1 = await context1.Customers.ToListAsync();
                Assert.Equal(91, customers1.Count);
                Assert.Equal(91, context1.ChangeTracker.Entries().Count());

                using var context2 = new NorthwindContext(serviceProvider);
                Assert.Empty(context2.ChangeTracker.Entries());

                var customers2 = await context2.Customers.ToListAsync();
                Assert.Equal(91, customers2.Count);
                Assert.Equal(91, context2.ChangeTracker.Entries().Count());

                Assert.Equal(customers1[0].CustomerID, customers2[0].CustomerID);
                Assert.NotSame(customers1[0], customers2[0]);
            }
        }

        private class NorthwindContext(IServiceProvider serviceProvider) : DbContext
        {
            private readonly IServiceProvider _serviceProvider = serviceProvider;

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => ConfigureModel(modelBuilder);

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseSqlServer(SqlServerNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());
        }
    }

    public class AzureSqlDatabase
    {
        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void Retry_on_failure_not_enabled_by_default_on_Azure_SQL(bool configured)
        {
            using var context = new NorthwindContext(configured);

            Assert.IsType<SqlServerExecutionStrategy>(context.Database.CreateExecutionStrategy());
        }

        private class NorthwindContext(bool configured) : DbContext
        {
            private readonly bool _azureConfigured = configured;

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableServiceProviderCaching(false)
                    .UseSqlServer(
                        @"Server=test.database.windows.net:4040;Database=Test;ConnectRetryCount=0",
                        a =>
                        {
                            if (_azureConfigured)
                            {
                                a.UseAzureSqlDefaults(false);
                            }
                        });

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => ConfigureModel(modelBuilder);
        }
    }

    public class NonDefaultAzureSqlDatabase
    {
        [InlineData(true)]
        [InlineData(false)]
        [ConditionalTheory]
        public void Retry_on_failure_enabled_if_Azure_SQL_configured(bool configured)
        {
            using var context = new NorthwindContext(configured);
            if (configured)
            {
                Assert.IsType<SqlServerRetryingExecutionStrategy>(context.Database.CreateExecutionStrategy());
            }
            else
            {
                Assert.IsType<SqlServerExecutionStrategy>(context.Database.CreateExecutionStrategy());
            }
        }

        private class NorthwindContext(bool azure) : DbContext
        {
            private readonly bool _isAzure = azure;

            public DbSet<Customer> Customers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableServiceProviderCaching(false)
                    .UseSqlServer(
                        SqlServerNorthwindTestStoreFactory.NorthwindConnectionString,
                        a =>
                        {
                            if (_isAzure)
                            {
                                a.UseAzureSqlDefaults();
                            }
                        });

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => ConfigureModel(modelBuilder);
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
