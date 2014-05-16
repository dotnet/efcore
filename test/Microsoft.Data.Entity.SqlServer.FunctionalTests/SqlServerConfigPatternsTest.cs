// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerConfigPatternsTest
    {
        public class ImplicitServicesAndConfig
        {
            [Fact]
            public async Task Can_query_with_implicit_services_and_OnConfiguring()
            {
                using (await TestDatabase.Northwind())
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

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ImplicitServicesExplicitConfig
        {
            [Fact]
            public async Task Can_query_with_implicit_services_and_explicit_config()
            {
                using (await TestDatabase.Northwind())
                {
                    var configuration = new DbContextOptions()
                        .UseSqlServer(TestDatabase.NorthwindConnectionString)
                        .BuildConfiguration();

                    using (var context = new NorthwindContext(configuration))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(ImmutableDbContextOptions configuration)
                    : base(configuration)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ExplicitServicesImplicitConfig
        {
            [Fact]
            public async Task Can_query_with_explicit_services_and_OnConfiguring()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework().AddSqlServer();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    using (var context = new NorthwindContext(serviceProvider))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ExplicitServicesAndConfig
        {
            [Fact]
            public async Task Can_query_with_explicit_services_and_explicit_config()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework().AddSqlServer();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    var configuration = new DbContextOptions()
                        .UseSqlServer(TestDatabase.NorthwindConnectionString)
                        .BuildConfiguration();

                    using (var context = new NorthwindContext(serviceProvider, configuration))
                    {
                        Assert.Equal(91, await context.Customers.CountAsync());
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                    : base(serviceProvider, configuration)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ExplicitServicesAndNoConfig
        {
            [Fact]
            public async Task Throws_on_attempt_to_use_SQL_Server_without_providing_connection_string()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework().AddSqlServer();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    Assert.Equal(
                        GetString("FormatNoDataStoreConfigured"),
                        Assert.Throws<InvalidOperationException>(() =>
                            {
                                using (var context = new NorthwindContext(serviceProvider))
                                {
                                    Assert.Equal(91, context.Customers.Count());
                                }
                            }).Message);
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class NoServicesAndNoConfig
        {
            [Fact]
            public async Task Throws_on_attempt_to_use_context_with_no_store()
            {
                using (await TestDatabase.Northwind())
                {
                    Assert.Equal(
                        GetString("FormatNoDataStoreConfigured"),
                        Assert.Throws<InvalidOperationException>(() =>
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

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ImplicitConfigButNoServices
        {
            [Fact]
            public async Task Throws_on_attempt_to_use_store_with_no_store_services()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    Assert.Equal(
                        GetString("FormatNoDataStoreService"),
                        Assert.Throws<InvalidOperationException>(() =>
                            {
                                using (var context = new NorthwindContext(serviceProvider))
                                {
                                    Assert.Equal(91, context.Customers.Count());
                                }
                            }).Message);
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class InjectContext
        {
            [Fact]
            public async Task Can_register_context_with_DI_container_and_have_it_injected()
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddEntityFramework().AddSqlServer();
                var serviceProvider = serviceCollection
                    .AddTransient<NorthwindContext>()
                    .AddTransient<MyController>()
                    .BuildServiceProvider();

                using (await TestDatabase.Northwind())
                {
                    await serviceProvider.GetService<MyController>().TestAsync();
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
                {
                    Assert.Equal(91, await _context.Customers.CountAsync());
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                    Assert.NotNull(serviceProvider);
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class InjectContextAndConfiguration
        {
            [Fact]
            public async Task Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
            {
                var configuration = new DbContextOptions()
                    .UseSqlServer(TestDatabase.NorthwindConnectionString)
                    .BuildConfiguration();

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddEntityFramework().AddSqlServer();
                serviceCollection
                    .AddTransient<MyController>()
                    .AddTransient<NorthwindContext>()
                    .AddInstance(configuration);
                var serviceProvider = serviceCollection.BuildServiceProvider();

                using (await TestDatabase.Northwind())
                {
                    await serviceProvider.GetService<MyController>().TestAsync();
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
                {
                    Assert.Equal(91, await _context.Customers.CountAsync());
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(IServiceProvider serviceProvider, ImmutableDbContextOptions configuration)
                    : base(serviceProvider, configuration)
                {
                    Assert.NotNull(serviceProvider);
                    Assert.NotNull(configuration);
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class InjectConfiguration
        {
            // This one is a bit strange because the context gets the configuration from the service provider
            // but doesn't get the service provider and so creates a new one for use internally. This works fine
            // although it would be much more common to inject both when using DI explicitly.
            [Fact]
            public async Task Can_register_configuration_with_DI_container_and_have_it_injected()
            {
                var configuration = new DbContextOptions()
                    .UseSqlServer(TestDatabase.NorthwindConnectionString)
                    .BuildConfiguration();

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddEntityFramework().AddSqlServer();
                serviceCollection
                    .AddTransient<NorthwindContext>()
                    .AddTransient<MyController>()
                    .AddInstance(configuration);
                var serviceProvider = serviceCollection.BuildServiceProvider();

                using (await TestDatabase.Northwind())
                {
                    await serviceProvider.GetService<MyController>().TestAsync();
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
                {
                    Assert.Equal(91, await QueryableExtensions.CountAsync(_context.Customers));
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(ImmutableDbContextOptions configuration)
                    : base(configuration)
                {
                    Assert.NotNull(configuration);
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ConstructorArgsToBuilder
        {
            [Fact]
            public async Task Can_pass_connection_string_to_constructor_and_use_in_builder()
            {
                using (await TestDatabase.Northwind())
                {
                    using (var context = new NorthwindContext(TestDatabase.NorthwindConnectionString))
                    {
                        Assert.Equal(91, await QueryableExtensions.CountAsync(context.Customers));
                    }
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext(string connectionString)
                    : base(new DbContextOptions()
                        .UseSqlServer(connectionString)
                        .BuildConfiguration())
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class ConstructorArgsToOnConfiguring
        {
            [Fact]
            public async Task Can_pass_connection_string_to_constructor_and_use_in_OnConfiguring()
            {
                using (await TestDatabase.Northwind())
                {
                    using (var context = new NorthwindContext(TestDatabase.NorthwindConnectionString))
                    {
                        Assert.Equal(91, await QueryableExtensions.CountAsync(context.Customers));
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

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(_connectionString);
                }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }
            }
        }

        public class NestedContext
        {
            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_the_same_type()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework().AddSqlServer();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    using (var context1 = new NorthwindContext(serviceProvider))
                    {
                        var customers1 = await context1.Customers.ToListAsync();
                        Assert.Equal(91, customers1.Count);
                        Assert.Equal(91, context1.ChangeTracker.Entries().Count());

                        using (var context2 = new NorthwindContext(serviceProvider))
                        {
                            Assert.Equal(0, context2.ChangeTracker.Entries().Count());

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
                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }
            }
        }

        public class NestedContextDifferentStores
        {
            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_a_different_type()
            {
                using (await TestDatabase.Northwind())
                {
                    var serviceCollection = new ServiceCollection();
                    serviceCollection.AddEntityFramework().AddSqlServer().AddInMemoryStore();
                    var serviceProvider = serviceCollection.BuildServiceProvider();

                    await NestedContextTest(() => new BlogContext(serviceProvider), () => new NorthwindContext(serviceProvider));
                }
            }

            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_a_different_type_with_implicit_services()
            {
                using (await TestDatabase.Northwind())
                {
                    await NestedContextTest(() => new BlogContext(), () => new NorthwindContext());
                }
            }

            private async Task NestedContextTest(Func<BlogContext> createBlogContext, Func<NorthwindContext> createNorthwindContext)
            {
                using (await TestDatabase.Northwind())
                {
                    using (var context0 = createBlogContext())
                    {
                        Assert.Equal(0, context0.ChangeTracker.Entries().Count());
                        var blog0 = context0.Add(new Blog { Id = 1, Name = "Giddyup" });
                        Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());
                        await context0.SaveChangesAsync();

                        using (var context1 = createNorthwindContext())
                        {
                            var customers1 = await context1.Customers.ToListAsync();
                            Assert.Equal(91, customers1.Count);
                            Assert.Equal(91, context1.ChangeTracker.Entries().Count());
                            Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());

                            using (var context2 = createBlogContext())
                            {
                                Assert.Equal(0, context2.ChangeTracker.Entries().Count());
                                Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());

                                var blog0Prime = (await context2.Blogs.ToArrayAsync()).Single();
                                Assert.Same(blog0Prime, context2.ChangeTracker.Entries().Select(e => e.Entity).Single());

                                Assert.Equal(blog0.Id, blog0Prime.Id);
                                Assert.NotSame(blog0, blog0Prime);
                            }
                        }
                    }
                }
            }

            private class BlogContext : DbContext
            {
                public BlogContext()
                {
                }

                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseInMemoryStore();
                }
            }

            private class NorthwindContext : DbContext
            {
                public NorthwindContext()
                {
                }

                public NorthwindContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder builder)
                {
                    ConfigureModel(builder);
                }

                protected override void OnConfiguring(DbContextOptions builder)
                {
                    builder.UseSqlServer(TestDatabase.NorthwindConnectionString);
                }
            }
        }

        private class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Customer
        {
            public string CustomerID { get; set; }
            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private static void ConfigureModel(ModelBuilder builder)
        {
            builder
                .Entity<Customer>()
                .Key(c => c.CustomerID)
                .StorageName("Customers");
        }

        private static string GetString(string stringName)
        {
            var strings = typeof(DbContext).GetTypeInfo().Assembly.GetType(typeof(DbContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, null);
        }
    }
}
