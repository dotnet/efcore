// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Xunit;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class ConfigPatternsInMemoryTest
    {
        [Fact]
        public void Can_save_and_query_with_implicit_services_and_OnConfiguring()
        {
            using (var context = new ImplicitServicesAndConfigBlogContext())
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
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
            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }

        [Fact]
        public void Can_save_and_query_with_implicit_services_and_explicit_config()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            using (var context = new ImplicitServicesExplicitConfigBlogContext(optionsBuilder.Options))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
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

        private class ImplicitServicesExplicitConfigBlogContext : DbContext
        {
            public ImplicitServicesExplicitConfigBlogContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_save_and_query_with_explicit_services_and_OnConfiguring()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryDatabase();
            var serviceProvider = services.BuildServiceProvider();

            using (var context = new ExplicitServicesImplicitConfigBlogContext(serviceProvider))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
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

        private class ExplicitServicesImplicitConfigBlogContext : DbContext
        {
            public ExplicitServicesImplicitConfigBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }

        [Fact]
        public void Can_save_and_query_with_explicit_services_and_explicit_config()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryDatabase();
            var serviceProvider = services.BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            using (var context = new ExplicitServicesAndConfigBlogContext(serviceProvider, optionsBuilder.Options))
            {
                context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                context.SaveChanges();
            }

            using (var context = new ExplicitServicesAndConfigBlogContext(serviceProvider, optionsBuilder.Options))
            {
                var blog = context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);

                context.Blogs.RemoveRange(context.Blogs);
                context.SaveChanges();

                Assert.Empty(context.Blogs);
            }
        }

        private class ExplicitServicesAndConfigBlogContext : DbContext
        {
            public ExplicitServicesAndConfigBlogContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Throws_on_attempt_to_use_context_with_no_store()
        {
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() =>
                    {
                        using (var context = new NoServicesAndNoConfigBlogContext())
                        {
                            context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                            context.SaveChanges();
                        }
                    }).Message);
        }

        private class NoServicesAndNoConfigBlogContext : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Throws_on_attempt_to_use_store_with_no_store_services()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Equal(
                CoreStrings.NoProviderServices,
                Assert.Throws<InvalidOperationException>(() =>
                    {
                        using (var context = new ImplicitConfigButNoServicesBlogContext(serviceProvider))
                        {
                            context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                            context.SaveChanges();
                        }
                    }).Message);
        }

        private class ImplicitConfigButNoServicesBlogContext : DbContext
        {
            public ImplicitConfigButNoServicesBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }

        [Fact]
        public void Can_register_context_with_DI_container_and_have_it_injected()
        {
            var services = new ServiceCollection();
            services.AddTransient<InjectContextBlogContext>()
                .AddTransient<InjectContextController>()
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

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
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectContextBlogContext : DbContext
        {
            public InjectContextBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var services = new ServiceCollection();
            services.AddTransient<InjectContextAndConfigurationBlogContext>()
                .AddTransient<InjectContextAndConfigurationController>()
                .AddInstance(optionsBuilder.Options)
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

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
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectContextAndConfigurationBlogContext : DbContext
        {
            public InjectContextAndConfigurationBlogContext(IServiceProvider serviceProvider, DbContextOptions options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        // This one is a bit strange because the context gets the configuration from the service provider
        // but doesn't get the service provider and so creates a new one for use internally. This works fine
        // although it would be much more common to inject both when using DI explicitly.
        [Fact]
        public void Can_register_configuration_with_DI_container_and_have_it_injected()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(persist: false);

            var services = new ServiceCollection();
            services.AddTransient<InjectConfigurationBlogContext>()
                .AddTransient<InjectConfigurationController>()
                .AddInstance(optionsBuilder.Options)
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

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
                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectConfigurationBlogContext : DbContext
        {
            public InjectConfigurationBlogContext(DbContextOptions options)
                : base(options)
            {
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        [Fact]
        public void Can_inject_different_configurations_into_different_contexts()
        {
            var blogOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsBlogContext>();
            blogOptions.UseInMemoryDatabase();

            var accountOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsAccountContext>();
            accountOptions.UseInMemoryDatabase();

            var services = new ServiceCollection();
            services.AddTransient<InjectDifferentConfigurationsBlogContext>()
                .AddTransient<InjectDifferentConfigurationsAccountContext>()
                .AddTransient<InjectDifferentConfigurationsBlogController>()
                .AddTransient<InjectDifferentConfigurationsAccountController>()
                .AddInstance(blogOptions.Options)
                .AddInstance(accountOptions.Options)
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

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

                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
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

                _context.Accounts.Add(new Account { Name = "Eeky Bear" });
                _context.SaveChanges();

                var account = _context.Accounts.SingleOrDefault();

                Assert.Equal(1, account.Id);
                Assert.Equal("Eeky Bear", account.Name);
            }
        }

        private class InjectDifferentConfigurationsBlogContext : DbContext
        {
            public InjectDifferentConfigurationsBlogContext(IServiceProvider serviceProvider, DbContextOptions<InjectDifferentConfigurationsBlogContext> options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        private class InjectDifferentConfigurationsAccountContext : DbContext
        {
            public InjectDifferentConfigurationsAccountContext(IServiceProvider serviceProvider, DbContextOptions<InjectDifferentConfigurationsAccountContext> options)
                : base(serviceProvider, options)
            {
                Assert.NotNull(serviceProvider);
                Assert.NotNull(options);
            }

            public DbSet<Account> Accounts { get; set; }
        }

        [Fact]
        public void Can_inject_different_configurations_into_different_contexts_without_declaring_in_constructor()
        {
            var blogOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsNoConstructorBlogContext>();
            blogOptions.UseInMemoryDatabase();

            var accountOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsNoConstructorAccountContext>();
            accountOptions.UseInMemoryDatabase();

            var services = new ServiceCollection();
            services.AddTransient<InjectDifferentConfigurationsNoConstructorBlogContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorBlogController>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountController>()
                .AddInstance(blogOptions.Options)
                .AddInstance(accountOptions.Options)
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<InjectDifferentConfigurationsNoConstructorBlogController>().Test();
            serviceProvider.GetRequiredService<InjectDifferentConfigurationsNoConstructorAccountController>().Test();
        }

        [Fact]
        public void Cannot_inject_different_configurations_into_different_contexts_both_as_base_type_without_constructor()
        {
            var blogOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsNoConstructorBlogContext>();
            blogOptions.UseInMemoryDatabase();

            var accountOptions = new DbContextOptionsBuilder<InjectDifferentConfigurationsNoConstructorAccountContext>();
            accountOptions.UseInMemoryDatabase();

            var services = new ServiceCollection();
            services.AddTransient<InjectDifferentConfigurationsNoConstructorBlogContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountContext>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorBlogController>()
                .AddTransient<InjectDifferentConfigurationsNoConstructorAccountController>()
                .AddInstance<DbContextOptions>(blogOptions.Options)
                .AddInstance<DbContextOptions>(accountOptions.Options)
                .AddEntityFramework()
                .AddInMemoryDatabase();

            var serviceProvider = services.BuildServiceProvider();

            Assert.Equal(
                CoreStrings.NonGenericOptions,
                Assert.Throws<InvalidOperationException>(
                    () => serviceProvider.GetRequiredService<InjectDifferentConfigurationsNoConstructorBlogController>().Test()).Message);
        }

        private class InjectDifferentConfigurationsNoConstructorBlogController
        {
            private readonly InjectDifferentConfigurationsNoConstructorBlogContext _context;

            public InjectDifferentConfigurationsNoConstructorBlogController(InjectDifferentConfigurationsNoConstructorBlogContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsNoConstructorBlogContext>>(
                    _context.GetService<IDbContextOptions>());

                _context.Blogs.Add(new Blog { Name = "The Waffle Cart" });
                _context.SaveChanges();

                var blog = _context.Blogs.SingleOrDefault();

                Assert.NotEqual(0, blog.Id);
                Assert.Equal("The Waffle Cart", blog.Name);
            }
        }

        private class InjectDifferentConfigurationsNoConstructorAccountController
        {
            private readonly InjectDifferentConfigurationsNoConstructorAccountContext _context;

            public InjectDifferentConfigurationsNoConstructorAccountController(InjectDifferentConfigurationsNoConstructorAccountContext context)
            {
                Assert.NotNull(context);

                _context = context;
            }

            public void Test()
            {
                Assert.IsType<DbContextOptions<InjectDifferentConfigurationsNoConstructorAccountContext>>(
                    _context.GetService<IDbContextOptions>());

                _context.Accounts.Add(new Account { Name = "Eeky Bear" });
                _context.SaveChanges();

                var account = _context.Accounts.SingleOrDefault();

                Assert.Equal(1, account.Id);
                Assert.Equal("Eeky Bear", account.Name);
            }
        }

        private class InjectDifferentConfigurationsNoConstructorBlogContext : DbContext
        {
            public InjectDifferentConfigurationsNoConstructorBlogContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        private class InjectDifferentConfigurationsNoConstructorAccountContext : DbContext
        {
            public InjectDifferentConfigurationsNoConstructorAccountContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
                Assert.NotNull(serviceProvider);
            }

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
}
