// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    [SqlServerConfiguredCondition]
    public class ConfigurationPatternsTest : IClassFixture<CrossStoreFixture>, IDisposable
    {
        public ConfigurationPatternsTest(CrossStoreFixture fixture)
        {
            Fixture = fixture;
            ExistingTestStore = Fixture.CreateTestStore(SqlServerTestStoreFactory.Instance, StoreName, Seed);
        }

        [ConditionalFact]
        public void Can_register_multiple_context_types()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<MultipleContext1>()
                .AddDbContext<MultipleContext2>()
                .BuildServiceProvider();

            using (var context = serviceProvider.GetRequiredService<MultipleContext1>())
            {
                Assert.True(context.SimpleEntities.Any());
            }

            using (var context = serviceProvider.GetRequiredService<MultipleContext2>())
            {
                Assert.False(context.SimpleEntities.Any());
            }
        }

        [ConditionalFact(Skip = "#18682")]
        public void Can_register_multiple_context_types_with_default_service_provider()
        {
            using (var context = new MultipleContext1(new DbContextOptions<MultipleContext1>()))
            {
                Assert.True(context.SimpleEntities.Any());
            }

            using (var context = new MultipleContext2(new DbContextOptions<MultipleContext2>()))
            {
                Assert.False(context.SimpleEntities.Any());
            }
        }

        private class MultipleContext1 : CrossStoreContext
        {
            private readonly DbContextOptions<MultipleContext1> _options;

            public MultipleContext1(DbContextOptions<MultipleContext1> options)
                : base(options)
            {
                _options = options;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(StoreName), b => b.ApplyConfiguration());

                Assert.NotSame(_options, optionsBuilder.Options);
            }
        }

        private class MultipleContext2 : CrossStoreContext
        {
            private readonly DbContextOptions<MultipleContext2> _options;

            public MultipleContext2(DbContextOptions<MultipleContext2> options)
                : base(options)
            {
                _options = options;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseInMemoryDatabase(StoreName);

                Assert.NotSame(_options, optionsBuilder.Options);
            }
        }

        [ConditionalFact]
        public void Can_select_appropriate_provider_when_multiple_registered()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddScoped<SomeService>()
                    .AddDbContext<MultipleProvidersContext>()
                    .BuildServiceProvider();

            MultipleProvidersContext context1;
            MultipleProvidersContext context2;

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (context1 = serviceScope.ServiceProvider.GetRequiredService<MultipleProvidersContext>())
                {
                    context1.UseSqlServer = true;

                    Assert.True(context1.SimpleEntities.Any());
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

                    Assert.False(context2.SimpleEntities.Any());
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

        [ConditionalFact(Skip = "#18682")]
        public void Can_select_appropriate_provider_when_multiple_registered_with_default_service_provider()
        {
            using (var context = new MultipleProvidersContext())
            {
                context.UseSqlServer = true;

                Assert.True(context.SimpleEntities.Any());
            }

            using (var context = new MultipleProvidersContext())
            {
                context.UseSqlServer = false;

                Assert.False(context.SimpleEntities.Any());
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Customer
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string CustomerID { get; set; }

            public string CompanyName { get; set; }
            public string Fax { get; set; }
        }

        private class MultipleProvidersContext : CrossStoreContext
        {
            // ReSharper disable once MemberCanBePrivate.Local
            public bool UseSqlServer { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (UseSqlServer)
                {
                    optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString(StoreName), b => b.ApplyConfiguration());
                }
                else
                {
                    optionsBuilder.UseInMemoryDatabase(StoreName);
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SomeService
        {
            public SomeService(MultipleProvidersContext context)
            {
                Context = context;
            }

            public MultipleProvidersContext Context { get; }
        }

        private CrossStoreFixture Fixture { get; }
        private TestStore ExistingTestStore { get; }
        private static readonly string StoreName = "CrossStoreConfigurationPatternsTest";

        private void Seed(CrossStoreContext context)
        {
            context.SimpleEntities.Add(new SimpleEntity { StringProperty = "Entity 1" });

            context.SaveChanges();
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose() => ExistingTestStore.Dispose();
#pragma warning restore xUnit1013 // Public method should be marked as test

        [SqlServerConfiguredCondition]
        public class NestedContextDifferentStores : IClassFixture<CrossStoreFixture>, IDisposable
        {
            public NestedContextDifferentStores(CrossStoreFixture fixture)
            {
                Fixture = fixture;
                ExistingTestStore = Fixture.CreateTestStore(SqlServerTestStoreFactory.Instance, StoreName, Seed);
            }

            [ConditionalFact]
            public async Task Can_use_one_context_nested_inside_another_of_a_different_type()
            {
                var inMemoryServiceProvider = InMemoryFixture.DefaultServiceProvider;
                var sqlServerServiceProvider = SqlServerFixture.DefaultServiceProvider;

                await NestedContextTest(
                    () => new BlogContext(inMemoryServiceProvider),
                    () => new ExternalProviderContext(sqlServerServiceProvider));
            }

            [ConditionalFact(Skip = "#18682")]
            public Task Can_use_one_context_nested_inside_another_of_a_different_type_with_implicit_services()
                => NestedContextTest(() => new BlogContext(), () => new ExternalProviderContext());

            private async Task NestedContextTest(Func<BlogContext> createBlogContext, Func<CrossStoreContext> createSimpleContext)
            {
                using (var context0 = createBlogContext())
                {
                    Assert.Empty(context0.ChangeTracker.Entries());
                    var blog0 = context0.Add(new Blog { Id = 1, Name = "Giddyup" }).Entity;
                    Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());
                    await context0.SaveChangesAsync();

                    using (var context1 = createSimpleContext())
                    {
                        var customers1 = await context1.SimpleEntities.ToListAsync();
                        Assert.Single(customers1);
                        Assert.Single(context1.ChangeTracker.Entries());
                        Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());

                        using (var context2 = createBlogContext())
                        {
                            Assert.Empty(context2.ChangeTracker.Entries());
                            Assert.Same(blog0, context0.ChangeTracker.Entries().Select(e => e.Entity).Single());

                            var blog0Prime = (await context2.Blogs.ToArrayAsync()).Single();
                            Assert.Same(blog0Prime, context2.ChangeTracker.Entries().Select(e => e.Entity).Single());

                            Assert.Equal(blog0.Id, blog0Prime.Id);
                            Assert.NotSame(blog0, blog0Prime);
                        }
                    }
                }
            }

            private CrossStoreFixture Fixture { get; }
            private TestStore ExistingTestStore { get; }
            private static readonly string StoreName = "CrossStoreNestedContextTest";

            private void Seed(CrossStoreContext context)
            {
                context.SimpleEntities.Add(new SimpleEntity { StringProperty = "Entity 1" });

                context.SaveChanges();
            }

#pragma warning disable xUnit1013 // Public method should be marked as test
            public void Dispose() => ExistingTestStore.Dispose();
#pragma warning restore xUnit1013 // Public method should be marked as test

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
                        .UseInMemoryDatabase(nameof(BlogContext))
                        .UseInternalServiceProvider(_serviceProvider);
            }

            private class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            private class ExternalProviderContext : CrossStoreContext
            {
                private readonly IServiceProvider _serviceProvider;

                public ExternalProviderContext()
                {
                }

                public ExternalProviderContext(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                    => optionsBuilder
                        .UseSqlServer(SqlServerTestStore.CreateConnectionString(StoreName), b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);
            }
        }
    }
}
