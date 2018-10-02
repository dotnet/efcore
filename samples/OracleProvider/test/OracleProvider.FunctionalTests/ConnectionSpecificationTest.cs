// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore
{
    public class ConnectionSpecificationTest
    {
        [Fact]
        public void Can_specify_connection_string_in_OnConfiguring()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddDbContext<StringInOnConfiguringContext>()
                    .BuildServiceProvider();

            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = serviceProvider.GetRequiredService<StringInOnConfiguringContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Can_specify_connection_string_in_OnConfiguring_with_default_service_provider()
        {
            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = new StringInOnConfiguringContext())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class StringInOnConfiguringContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle(OracleNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());
        }

        [Fact]
        public void Can_specify_connection_in_OnConfiguring()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddScoped(_ => new OracleConnection(OracleNorthwindTestStoreFactory.NorthwindConnectionString))
                    .AddDbContext<ConnectionInOnConfiguringContext>().BuildServiceProvider();

            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = serviceProvider.GetRequiredService<ConnectionInOnConfiguringContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Can_specify_connection_in_OnConfiguring_with_default_service_provider()
        {
            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = new ConnectionInOnConfiguringContext(new OracleConnection(OracleNorthwindTestStoreFactory.NorthwindConnectionString)))
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class ConnectionInOnConfiguringContext : NorthwindContextBase
        {
            private readonly OracleConnection _connection;

            public ConnectionInOnConfiguringContext(OracleConnection connection)
            {
                _connection = connection;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle(_connection, b => b.ApplyConfiguration());

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        // ReSharper disable once UnusedMember.Local
        private class StringInConfigContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseOracle("Database=Crunchie", b => b.ApplyConfiguration());
        }

        [Fact]
        public void Throws_if_no_connection_found_in_config_without_UseOracle()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddDbContext<NoUseOracleContext>().BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<NoUseOracleContext>())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        [Fact]
        public void Throws_if_no_config_without_UseOracle()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddDbContext<NoUseOracleContext>().BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<NoUseOracleContext>())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        private class NoUseOracleContext : NorthwindContextBase
        {
        }

        [Fact]
        public void Can_depend_on_DbContextOptions()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddScoped(_ => new OracleConnection(OracleNorthwindTestStoreFactory.NorthwindConnectionString))
                    .AddDbContext<OptionsContext>()
                    .BuildServiceProvider();

            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = serviceProvider.GetRequiredService<OptionsContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Can_depend_on_DbContextOptions_with_default_service_provider()
        {
            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = new OptionsContext(
                    new DbContextOptions<OptionsContext>(),
                    new OracleConnection(OracleNorthwindTestStoreFactory.NorthwindConnectionString)))
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class OptionsContext : NorthwindContextBase
        {
            private readonly OracleConnection _connection;
            private readonly DbContextOptions<OptionsContext> _options;

            public OptionsContext(DbContextOptions<OptionsContext> options, OracleConnection connection)
                : base(options)
            {
                _options = options;
                _connection = connection;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseOracle(_connection, b => b.ApplyConfiguration());

                Assert.NotSame(_options, optionsBuilder.Options);
            }

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        [Fact]
        public void Can_depend_on_non_generic_options_when_only_one_context()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddDbContext<NonGenericOptionsContext>()
                    .BuildServiceProvider();

            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = serviceProvider.GetRequiredService<NonGenericOptionsContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Can_depend_on_non_generic_options_when_only_one_context_with_default_service_provider()
        {
            using (OracleTestStore.GetNorthwindStore())
            {
                using (var context = new NonGenericOptionsContext(new DbContextOptions<DbContext>()))
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class NonGenericOptionsContext : NorthwindContextBase
        {
            private readonly DbContextOptions _options;

            public NonGenericOptionsContext(DbContextOptions options)
                : base(options)
            {
                _options = options;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseOracle(OracleNorthwindTestStoreFactory.NorthwindConnectionString, b => b.ApplyConfiguration());

                Assert.NotSame(_options, optionsBuilder.Options);
            }
        }

        [Theory]
        [InlineData("MyConnectuonString", "name=MyConnectuonString")]
        [InlineData("ConnectionStrings:DefaultConnection", "name=ConnectionStrings:DefaultConnection")]
        [InlineData("ConnectionStrings:DefaultConnection", " NamE   =   ConnectionStrings:DefaultConnection  ")]
        public void Can_use_AddDbContext_and_get_connection_string_from_config(string key, string connectionString)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { key, OracleNorthwindTestStoreFactory.NorthwindConnectionString }
                    });

            var serviceProvider
                = new ServiceCollection()
                    .AddSingleton<IConfiguration>(configBuilder.Build())
                    .AddDbContext<UseConfigurationContext>(
                        b => b.UseOracle(connectionString))
                    .BuildServiceProvider();

            using (OracleTestStore.GetNorthwindStore())
            {
                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetRequiredService<UseConfigurationContext>())
                    {
                        Assert.True(context.Customers.Any());
                    }
                }
            }
        }

        private class UseConfigurationContext : NorthwindContextBase
        {
            public UseConfigurationContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class NorthwindContextBase : DbContext
        {
            protected NorthwindContextBase()
            {
            }

            protected NorthwindContextBase(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    b =>
                        {
                            b.HasKey(c => c.CustomerID);
                            b.ToTable("Customers");
                        });
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }

            // ReSharper disable UnusedMember.Local
            public string CompanyName { get; set; }

            public string Fax { get; set; }
            // ReSharper restore UnusedMember.Local
        }
    }
}
