// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class SqlServerConfigPatternsTest
    {
        public class ImplicitServicesAndConfig
        {
            [Fact]
            public async Task Can_query_with_implicit_services_and_OnConfiguring()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
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
                    => optionsBuilder.UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ImplicitServicesExplicitConfig
        {
            [Fact]
            public async Task Can_query_with_implicit_services_and_explicit_config()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    using (var context = new NorthwindContext(
                        new DbContextOptionsBuilder()
                            .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration()).Options))
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
            [Fact]
            public async Task Can_query_with_explicit_services_and_OnConfiguring()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
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
                    => optionsBuilder.UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ExplicitServicesAndConfig
        {
            [Fact]
            public async Task Can_query_with_explicit_services_and_explicit_config()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    using (var context = new NorthwindContext(new DbContextOptionsBuilder()
                        .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(new ServiceCollection()
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
            [Fact]
            public void Throws_on_attempt_to_use_SQL_Server_without_providing_connection_string()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
                        Assert.Throws<InvalidOperationException>(() =>
                            {
                                using (var context = new NorthwindContext(
                                    new DbContextOptionsBuilder().UseInternalServiceProvider(new ServiceCollection()
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
            [Fact]
            public void Throws_on_attempt_to_use_context_with_no_store()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
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

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class ImplicitConfigButNoServices
        {
            [Fact]
            public void Throws_on_attempt_to_use_store_with_no_store_services()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    Assert.Equal(
                        CoreStrings.NoProviderConfigured,
                        Assert.Throws<InvalidOperationException>(() =>
                            {
                                using (var context = new NorthwindContext(
                                    new DbContextOptionsBuilder()
                                        .UseInternalServiceProvider(new ServiceCollection()
                                            .AddEntityFramework()
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

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
                    optionsBuilder.UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class InjectContext
        {
            [Fact]
            public async Task Can_register_context_with_DI_container_and_have_it_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddTransient<NorthwindContext>()
                    .AddTransient<MyController>()
                    .AddSingleton(p => new DbContextOptionsBuilder().UseInternalServiceProvider(p).Options)
                    .BuildServiceProvider();

                using (SqlServerNorthwindContext.GetSharedStore())
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
                    => optionsBuilder.UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class InjectContextAndConfiguration
        {
            [Fact]
            public async Task Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddTransient<MyController>()
                    .AddTransient<NorthwindContext>()
                    .AddSingleton(new DbContextOptionsBuilder()
                        .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration()).Options).BuildServiceProvider();

                using (SqlServerNorthwindContext.GetSharedStore())
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
            [Fact]
            public async Task Can_pass_context_options_to_constructor_and_use_in_builder()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    using (var context = new NorthwindContext(new DbContextOptionsBuilder()
                        .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration()).Options))
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
            [Fact]
            public async Task Can_pass_connection_string_to_constructor_and_use_in_OnConfiguring()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    using (var context = new NorthwindContext(SqlServerNorthwindContext.ConnectionString))
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
                    => optionsBuilder.UseSqlServer(_connectionString, b => b.ApplyConfiguration());

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);
            }
        }

        public class NestedContext
        {
            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_the_same_type()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
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
                    .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration());
            }
        }

        public class NestedContextDifferentStores
        {
            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_a_different_type()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    await NestedContextTest(() => new BlogContext(serviceProvider), () => new NorthwindContext(serviceProvider));
                }
            }

            [Fact]
            public async Task Can_use_one_context_nested_inside_another_of_a_different_type_with_implicit_services()
            {
                using (SqlServerNorthwindContext.GetSharedStore())
                {
                    await NestedContextTest(() => new BlogContext(), () => new NorthwindContext());
                }
            }

            private async Task NestedContextTest(Func<BlogContext> createBlogContext, Func<NorthwindContext> createNorthwindContext)
            {
                using (var context0 = createBlogContext())
                {
                    Assert.Equal(0, context0.ChangeTracker.Entries().Count());
                    var blog0 = context0.Add(new Blog { Id = 1, Name = "Giddyup" }).Entity;
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

            private class BlogContext : DbContext
            {
                private readonly IServiceProvider _serviceProvider;

                public BlogContext()
                {
                }

                public BlogContext(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                public DbSet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder
                        .UseInMemoryDatabase()
                        .UseInternalServiceProvider(_serviceProvider);
            }

            private class NorthwindContext : DbContext
            {
                private readonly IServiceProvider _serviceProvider;

                public NorthwindContext()
                {
                }

                public NorthwindContext(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                public DbSet<Customer> Customers { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                    => ConfigureModel(modelBuilder);

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder
                        .UseSqlServer(SqlServerNorthwindContext.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);
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
            => builder.Entity<Customer>(b =>
                {
                    b.HasKey(c => c.CustomerID);
                    b.ForSqlServerToTable("Customers");
                });
    }
}
