// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;
using RelationalStrings = Microsoft.Data.Entity.Relational.Strings;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class ConnectionSpecificationTest
    {
        [Fact]
        public async void Can_specify_connection_string_in_OnConfiguring()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<StringInOnConfiguringContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<StringInOnConfiguringContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_specify_connection_string_in_OnConfiguring_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new StringInOnConfiguringContext())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class StringInOnConfiguringContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerNorthwindContext.ConnectionString);
            }
        }

        [Fact]
        public async void Can_specify_connection_in_OnConfiguring()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddScoped<SqlConnection>(p => new SqlConnection(SqlServerNorthwindContext.ConnectionString))
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ConnectionInOnConfiguringContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<ConnectionInOnConfiguringContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_specify_connection_in_OnConfiguring_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new ConnectionInOnConfiguringContext(new SqlConnection(SqlServerNorthwindContext.ConnectionString)))
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class ConnectionInOnConfiguringContext : NorthwindContextBase
        {
            private readonly SqlConnection _connection;

            public ConnectionInOnConfiguringContext(SqlConnection connection)
            {
                _connection = connection;
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(_connection);
            }

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        [Fact]
        public async void Can_specify_dereferenced_connection_string_in_config()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            {
                                "Data:DefaultConnection:ConnectionString", SqlServerNorthwindContext.ConnectionString
                            },
                            {
                                "EntityFramework:" + typeof(StringInConfigContext).Name + ":ConnectionString", "Name=Data:DefaultConnection:ConnectionString"
                            }
                        }
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<StringInConfigContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<StringInConfigContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_specify_connection_string_in_config()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            {
                                "EntityFramework:" + typeof(StringInConfigContext).Name + ":ConnectionString", SqlServerNorthwindContext.ConnectionString
                            }
                        }
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<StringInConfigContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<StringInConfigContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Throws_if_no_connection_found_in_config()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource()
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<StringInConfigContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<StringInConfigContext>())
            {
                Assert.Equal(
                    RelationalStrings.NoConnectionOrConnectionString,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        [Fact]
        public void Throws_if_no_config()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<StringInConfigContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<StringInConfigContext>())
            {
                Assert.Equal(
                    RelationalStrings.NoConnectionOrConnectionString,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        [Fact]
        public void Throws_if_no_config_with_default_service_provider()
        {
            using (var context = new StringInConfigContext())
            {
                Assert.Equal(
                    RelationalStrings.NoConnectionOrConnectionString,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        private class StringInConfigContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer();
            }
        }

        [Fact]
        public async void Can_specify_dereferenced_connection_string_in_config_without_UseSqlServer()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            {
                                "Data:DefaultConnection:ConnectionString", SqlServerNorthwindContext.ConnectionString
                            },
                            {
                                "EntityFramework:" + typeof(NoUseSqlServerContext).Name + ":ConnectionString", "Name=Data:DefaultConnection:ConnectionString"
                            }
                        }
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<NoUseSqlServerContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<NoUseSqlServerContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_specify_connection_string_in_config_without_UseSqlServer()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource
                        {
                            {
                                "EntityFramework:" + typeof(NoUseSqlServerContext).Name + ":ConnectionString", SqlServerNorthwindContext.ConnectionString
                            }
                        }
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<NoUseSqlServerContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<NoUseSqlServerContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public void Throws_if_no_connection_found_in_config_without_UseSqlServer()
        {
            var configuration = new Configuration
                {
                    new MemoryConfigurationSource()
                };

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework(configuration)
                .AddSqlServer()
                .AddDbContext<NoUseSqlServerContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<NoUseSqlServerContext>())
            {
                Assert.Equal(
                    RelationalStrings.NoConnectionOrConnectionString,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        [Fact]
        public void Throws_if_no_config_without_UseSqlServer()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<NoUseSqlServerContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<NoUseSqlServerContext>())
            {
                Assert.Equal(
                    RelationalStrings.NoConnectionOrConnectionString,
                    Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
            }
        }

        private class NoUseSqlServerContext : NorthwindContextBase
        {
        }

        [Fact]
        public async void Can_select_appropriate_provider_when_multiple_registered()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddScoped<SomeService>()
                .AddEntityFramework()
                .AddSqlServer()
                .AddInMemoryStore()
                .AddDbContext<MultipleProvidersContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                MultipleProvidersContext context1;
                MultipleProvidersContext context2;

                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    using (context1 = serviceScope.ServiceProvider.GetRequiredService<MultipleProvidersContext>())
                    {
                        context1.UseSqlServer = true;

                        Assert.True(context1.Customers.Any());
                    }

                    using (var context1B = serviceScope.ServiceProvider.GetRequiredService<MultipleProvidersContext>())
                    {
                        Assert.Same(context1, context1B);
                    }

                    var someService = serviceScope.ServiceProvider.GetRequiredService<SomeService>();
                    Assert.Same(context1, someService.Context);
                }
                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    using (context2 = serviceScope.ServiceProvider.GetRequiredService<MultipleProvidersContext>())
                    {
                        context2.UseSqlServer = false;

                        Assert.False(context2.Customers.Any());
                    }

                    using (var context2B = serviceScope.ServiceProvider.GetRequiredService<MultipleProvidersContext>())
                    {
                        Assert.Same(context2, context2B);
                    }

                    var someService = serviceScope.ServiceProvider.GetRequiredService<SomeService>();
                    Assert.Same(context2, someService.Context);
                }

                Assert.NotSame(context1, context2);
            }
        }

        [Fact]
        public async void Can_select_appropriate_provider_when_multiple_registered_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new MultipleProvidersContext())
                {
                    context.UseSqlServer = true;

                    Assert.True(context.Customers.Any());
                }

                using (var context = new MultipleProvidersContext())
                {
                    context.UseSqlServer = false;

                    Assert.False(context.Customers.Any());
                }
            }
        }

        private class MultipleProvidersContext : NorthwindContextBase
        {
            public bool UseSqlServer { get; set; }

            protected override void OnConfiguring(DbContextOptions options)
            {
                if (UseSqlServer)
                {
                    options.UseSqlServer(SqlServerNorthwindContext.ConnectionString);
                }
                else
                {
                    options.UseInMemoryStore();
                }
            }
        }

        private class SomeService
        {
            public SomeService(MultipleProvidersContext context)
            {
                Context = context;
            }

            public MultipleProvidersContext Context { get; set; }
        }

        [Fact]
        public async void Can_depend_on_DbContextOptions()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddScoped<SqlConnection>(p => new SqlConnection(SqlServerNorthwindContext.ConnectionString))
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<OptionsContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<OptionsContext>())
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_depend_on_DbContextOptions_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new OptionsContext(
                    new DbContextOptions<OptionsContext>(),
                    new SqlConnection(SqlServerNorthwindContext.ConnectionString)))
                {
                    Assert.True(context.Customers.Any());
                }
            }
        }

        private class OptionsContext : NorthwindContextBase
        {
            private readonly SqlConnection _connection;
            private readonly DbContextOptions<OptionsContext> _options;

            public OptionsContext(DbContextOptions<OptionsContext> options, SqlConnection connection)
                : base(options)
            {
                _options = options;
                _connection = connection;

                ((IDbContextOptions)_options).AddExtension(new FakeDbContextOptionsExtension());
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(_connection);

                // Options was cloned
                Assert.NotSame(options, _options);

                Assert.Equal(1, ((IDbContextOptions)options).Extensions.OfType<FakeDbContextOptionsExtension>().Count());
            }

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        [Fact]
        public async void Can_register_multiple_context_types()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddInMemoryStore()
                .AddDbContext<MultipleContext1>()
                .AddDbContext<MultipleContext2>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = serviceProvider.GetRequiredService<MultipleContext1>())
                {
                    Assert.True(context.Customers.Any());
                }

                using (var context = serviceProvider.GetRequiredService<MultipleContext2>())
                {
                    Assert.False(context.Customers.Any());
                }
            }
        }

        [Fact]
        public async void Can_register_multiple_context_types_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new MultipleContext1(new DbContextOptions<MultipleContext1>()))
                {
                    Assert.True(context.Customers.Any());
                }

                using (var context = new MultipleContext2(new DbContextOptions<MultipleContext2>()))
                {
                    Assert.False(context.Customers.Any());
                }
            }
        }

        private class MultipleContext1 : NorthwindContextBase
        {
            private readonly DbContextOptions<MultipleContext1> _options;

            public MultipleContext1(DbContextOptions<MultipleContext1> options)
                : base(options)
            {
                _options = options;
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerNorthwindContext.ConnectionString);

                // Options was cloned
                Assert.NotSame(options, _options);
            }
        }

        private class MultipleContext2 : NorthwindContextBase
        {
            private readonly DbContextOptions<MultipleContext2> _options;

            public MultipleContext2(DbContextOptions<MultipleContext2> options)
                : base(options)
            {
                _options = options;
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();

                // Options was cloned
                Assert.NotSame(options, _options);
            }
        }

        [Fact]
        public async void Can_depend_on_non_generic_options_when_only_one_context()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddInMemoryStore()
                .AddDbContext<NonGenericOptionsContext>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                // TODO: Make this work or provide better exception
                // Issue #935
                Assert.Throws<InvalidOperationException>(()
                    =>
                    {
                        using (var context = serviceProvider.GetRequiredService<NonGenericOptionsContext>())
                        {
                            Assert.True(context.Customers.Any());
                        }
                    });
            }
        }

        [Fact]
        public async void Can_depend_on_non_generic_options_when_only_one_context_with_default_service_provider()
        {
            using (await SqlServerNorthwindContext.GetSharedStoreAsync())
            {
                using (var context = new NonGenericOptionsContext(new DbContextOptions()))
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

                ((IDbContextOptions)_options).AddExtension(new FakeDbContextOptionsExtension());
            }

            protected override void OnConfiguring(DbContextOptions options)
            {
                options.UseSqlServer(SqlServerNorthwindContext.ConnectionString);

                // Options was cloned
                Assert.NotSame(options, _options);

                Assert.Equal(1, ((IDbContextOptions)options).Extensions.OfType<FakeDbContextOptionsExtension>().Count());
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
                modelBuilder.Entity<Customer>(b =>
                    {
                        b.Key(c => c.CustomerID);
                        b.ForSqlServer().Table("Customers");
                    });
            }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private class FakeDbContextOptionsExtension : DbContextOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }
    }
}
