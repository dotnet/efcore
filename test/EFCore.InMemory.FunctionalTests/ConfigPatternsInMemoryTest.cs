// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ConfigPatternsInMemoryTest
{
    [ConditionalFact]
    public void Can_save_and_query_with_implicit_services_and_OnConfiguring()
    {
        using (var context = new ImplicitServicesAndConfigBlogContext())
        {
            context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            context.SaveChanges();
        }

        using (var context = new ImplicitServicesAndConfigBlogContext())
        {
            var blog = context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);

            context.Blogs.RemoveRange(context.Blogs);
            context.SaveChanges();

            Assert.Empty(context.Blogs);
        }
    }

    private class ImplicitServicesAndConfigBlogContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(ImplicitServicesAndConfigBlogContext));
    }

    [ConditionalFact]
    public void Can_save_and_query_with_implicit_services_and_explicit_config()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseInMemoryDatabase(nameof(ImplicitServicesExplicitConfigBlogContext));

        using (var context = new ImplicitServicesExplicitConfigBlogContext(optionsBuilder.Options))
        {
            context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            context.SaveChanges();
        }

        using (var context = new ImplicitServicesExplicitConfigBlogContext(optionsBuilder.Options))
        {
            var blog = context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);

            context.Blogs.RemoveRange(context.Blogs);
            context.SaveChanges();

            Assert.Empty(context.Blogs);
        }
    }

    private class ImplicitServicesExplicitConfigBlogContext(DbContextOptions options) : DbContext(options)
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public void Can_save_and_query_with_explicit_services_and_OnConfiguring()
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkInMemoryDatabase();
        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        using (var context = new ExplicitServicesImplicitConfigBlogContext(serviceProvider))
        {
            context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            context.SaveChanges();
        }

        using (var context = new ExplicitServicesImplicitConfigBlogContext(serviceProvider))
        {
            var blog = context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);

            context.Blogs.RemoveRange(context.Blogs);
            context.SaveChanges();

            Assert.Empty(context.Blogs);
        }
    }

    private class ExplicitServicesImplicitConfigBlogContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(ExplicitServicesImplicitConfigBlogContext));
    }

    [ConditionalFact]
    public void Can_save_and_query_with_explicit_services_and_explicit_config()
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(nameof(ExplicitServicesAndConfigBlogContext))
            .UseInternalServiceProvider(
                new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true));

        using (var context = new ExplicitServicesAndConfigBlogContext(optionsBuilder.Options))
        {
            context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            context.SaveChanges();
        }

        using (var context = new ExplicitServicesAndConfigBlogContext(optionsBuilder.Options))
        {
            var blog = context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);

            context.Blogs.RemoveRange(context.Blogs);
            context.SaveChanges();

            Assert.Empty(context.Blogs);
        }
    }

    private class ExplicitServicesAndConfigBlogContext(DbContextOptions options) : DbContext(options)
    {

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public void Throws_on_attempt_to_use_context_with_no_store()
        => Assert.Equal(
            CoreStrings.NoProviderConfigured,
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    using var context = new NoServicesAndNoConfigBlogContext();
                    context.Blogs.Add(
                        new Blog { Name = "The Waffle Cart" });
                    context.SaveChanges();
                }).Message);

    private class NoServicesAndNoConfigBlogContext : DbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.EnableServiceProviderCaching(false);
    }

    [ConditionalFact]
    public void Throws_on_attempt_to_use_store_with_no_store_services()
    {
        var serviceCollection = new ServiceCollection();
        new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
        var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

        Assert.Equal(
            CoreStrings.NoProviderConfigured,
            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    using var context = new ImplicitConfigButNoServicesBlogContext(serviceProvider);
                    context.Blogs.Add(
                        new Blog { Name = "The Waffle Cart" });
                    context.SaveChanges();
                }).Message);
    }

    private class ImplicitConfigButNoServicesBlogContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(ImplicitConfigButNoServicesBlogContext))
                .UseInternalServiceProvider(_serviceProvider);
    }

    [ConditionalFact]
    public void Can_register_context_with_DI_container_and_have_it_injected()
    {
        var services = new ServiceCollection();
        services.AddTransient<InjectContextBlogContext>()
            .AddTransient<InjectContextController>()
            .AddEntityFrameworkInMemoryDatabase();

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        serviceProvider.GetRequiredService<InjectContextController>().Test();
    }

    private class InjectContextController
    {
        private readonly InjectContextBlogContext _context;

        public InjectContextController(InjectContextBlogContext context)
        {
            Assert.NotNull(context);

            _context = context;
        }

        public void Test()
        {
            _context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            _context.SaveChanges();

            var blog = _context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);
        }
    }

    private class InjectContextBlogContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public InjectContextBlogContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Assert.NotNull(serviceProvider);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(InjectContextBlogContext))
                .UseInternalServiceProvider(_serviceProvider);

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public void Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
    {
        var optionsBuilder = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(nameof(InjectContextAndConfigurationBlogContext));

        var serviceProvider = new ServiceCollection()
            .AddTransient<InjectContextAndConfigurationBlogContext>()
            .AddTransient<InjectContextAndConfigurationController>()
            .AddSingleton(p => optionsBuilder.UseInternalServiceProvider(p).Options)
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        serviceProvider.GetRequiredService<InjectContextAndConfigurationController>().Test();
    }

    private class InjectContextAndConfigurationController
    {
        private readonly InjectContextAndConfigurationBlogContext _context;

        public InjectContextAndConfigurationController(InjectContextAndConfigurationBlogContext context)
        {
            Assert.NotNull(context);

            _context = context;
        }

        public void Test()
        {
            _context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            _context.SaveChanges();

            var blog = _context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);
        }
    }

    private class InjectContextAndConfigurationBlogContext : DbContext
    {
        public InjectContextAndConfigurationBlogContext(DbContextOptions options)
            : base(options)
        {
            Assert.NotNull(options);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public void Can_register_configuration_with_DI_container_and_have_it_injected()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder
            .EnableServiceProviderCaching(false)
            .UseInMemoryDatabase(nameof(InjectConfigurationBlogContext));

        var services = new ServiceCollection();
        services.AddTransient<InjectConfigurationBlogContext>()
            .AddTransient<InjectConfigurationController>()
            .AddSingleton(optionsBuilder.Options)
            .AddEntityFrameworkInMemoryDatabase();

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        serviceProvider.GetRequiredService<InjectConfigurationController>().Test();
    }

    private class InjectConfigurationController
    {
        private readonly InjectConfigurationBlogContext _context;

        public InjectConfigurationController(InjectConfigurationBlogContext context)
        {
            Assert.NotNull(context);

            _context = context;
        }

        public void Test()
        {
            _context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            _context.SaveChanges();

            var blog = _context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);

            _context.Remove(blog);
            _context.SaveChanges();
        }
    }

    private class InjectConfigurationBlogContext : DbContext
    {
        public InjectConfigurationBlogContext(DbContextOptions options)
            : base(options)
        {
            Assert.NotNull(options);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    [ConditionalFact]
    public void Can_inject_different_configurations_into_different_contexts()
    {
        var blogOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsBlogContext>()
            .UseInMemoryDatabase(nameof(InjectDifferentConfigurationsBlogContext));

        var accountOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsAccountContext>()
            .UseInMemoryDatabase(nameof(InjectDifferentConfigurationsAccountContext));

        var serviceProvider = new ServiceCollection()
            .AddTransient<InjectDifferentConfigurationsBlogContext>()
            .AddTransient<InjectDifferentConfigurationsAccountContext>()
            .AddTransient<InjectDifferentConfigurationsBlogController>()
            .AddTransient<InjectDifferentConfigurationsAccountController>()
            .AddSingleton(p => blogOptions.UseInternalServiceProvider(p).Options)
            .AddSingleton(p => accountOptions.UseInternalServiceProvider(p).Options)
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        serviceProvider.GetRequiredService<InjectDifferentConfigurationsBlogController>().Test();
        serviceProvider.GetRequiredService<InjectDifferentConfigurationsAccountController>().Test();
    }

    private class InjectDifferentConfigurationsBlogController
    {
        private readonly InjectDifferentConfigurationsBlogContext _context;

        public InjectDifferentConfigurationsBlogController(InjectDifferentConfigurationsBlogContext context)
        {
            Assert.NotNull(context);

            _context = context;
        }

        public void Test()
        {
            Assert.IsType<DbContextOptions<InjectDifferentConfigurationsBlogContext>>(
                _context.GetService<IDbContextOptions>());

            _context.Blogs.Add(
                new Blog { Name = "The Waffle Cart" });
            _context.SaveChanges();

            var blog = _context.Blogs.SingleOrDefault();

            Assert.NotEqual(0, blog.Id);
            Assert.Equal("The Waffle Cart", blog.Name);
        }
    }

    private class InjectDifferentConfigurationsAccountController
    {
        private readonly InjectDifferentConfigurationsAccountContext _context;

        public InjectDifferentConfigurationsAccountController(InjectDifferentConfigurationsAccountContext context)
        {
            Assert.NotNull(context);

            _context = context;
        }

        public void Test()
        {
            Assert.IsType<DbContextOptions<InjectDifferentConfigurationsAccountContext>>(
                _context.GetService<IDbContextOptions>());

            _context.Accounts.Add(
                new Account { Name = "Eeky Bear" });
            _context.SaveChanges();

            var account = _context.Accounts.SingleOrDefault();

            Assert.Equal(1, account.Id);
            Assert.Equal("Eeky Bear", account.Name);
        }
    }

    private class InjectDifferentConfigurationsBlogContext : DbContext
    {
        public InjectDifferentConfigurationsBlogContext(DbContextOptions<InjectDifferentConfigurationsBlogContext> options)
            : base(options)
        {
            Assert.NotNull(options);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Blog> Blogs { get; set; }
    }

    private class InjectDifferentConfigurationsAccountContext : DbContext
    {
        public InjectDifferentConfigurationsAccountContext(DbContextOptions<InjectDifferentConfigurationsAccountContext> options)
            : base(options)
        {
            Assert.NotNull(options);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Account> Accounts { get; set; }
    }

    private class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
