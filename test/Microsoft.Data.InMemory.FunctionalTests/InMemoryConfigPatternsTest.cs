// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class InMemoryConfigPatternsTest
    {
        public class ImplicitServicesAndConfig
        {
            [Fact]
            public void Can_save_and_query_with_implicit_services_and_OnConfiguring()
            {
                using (var context = new BlogContext())
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext())
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public EntitySet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(EntityConfigurationBuilder builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class ImplicitServicesExplicitConfig
        {
            [Fact]
            public void Can_save_and_query_with_implicit_services_and_explicit_config()
            {
                var configuration = new EntityConfigurationBuilder()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                using (var context = new BlogContext(configuration))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(configuration))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(EntityConfiguration configuration)
                    : base(configuration)
                {
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class ExplicitServicesImplicitConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_OnConfiguring()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                using (var context = new BlogContext(serviceProvider))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public EntitySet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(EntityConfigurationBuilder builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class ExplicitServicesAndConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_explicit_config()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                var configuration = new EntityConfigurationBuilder()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                using (var context = new BlogContext(serviceProvider, configuration))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider, configuration))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider, EntityConfiguration configuration)
                    : base(serviceProvider, configuration)
                {
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class ExplicitServicesAndNoConfig
        {
            [Fact]
            public void Can_save_and_query_with_explicit_services_and_no_config()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider();

                using (var context = new BlogContext(serviceProvider))
                {
                    context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    context.SaveChanges();
                }

                using (var context = new BlogContext(serviceProvider))
                {
                    var blog = context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class NoServicesAndNoConfig
        {
            [Fact]
            public void Throws_on_attempt_to_use_context_with_no_store()
            {
                Assert.Equal(
                    GetString("FormatNoDataStoreConfigured"),
                    // TODO: Should not be AggregateException
                    Assert.Throws<AggregateException>(() =>
                        {
                            using (var context = new BlogContext())
                            {
                                context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                                context.SaveChanges();
                            }
                        }).InnerException.Message);
            }

            private class BlogContext : EntityContext
            {
                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class ImplicitConfigButNoServices
        {
            [Fact]
            public void Throws_on_attempt_to_use_store_with_no_store_services()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .BuildServiceProvider();

                Assert.Equal(
                    GetString("FormatNoDataStoreService"),
                    // TODO: Should not be AggregateException
                    Assert.Throws<AggregateException>(() =>
                        {
                            using (var context = new BlogContext(serviceProvider))
                            {
                                context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                                context.SaveChanges();
                            }
                        }).InnerException.Message);
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                }

                public EntitySet<Blog> Blogs { get; set; }

                protected override void OnConfiguring(EntityConfigurationBuilder builder)
                {
                    builder.UseInMemoryStore();
                }
            }
        }

        public class InjectContext
        {
            [Fact]
            public void Can_register_context_with_DI_container_and_have_it_injected()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider)
                    : base(serviceProvider)
                {
                    Assert.NotNull(serviceProvider);
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class InjectContextAndConfiguration
        {
            [Fact]
            public void Can_register_context_and_configuration_with_DI_container_and_have_both_injected()
            {
                var configuration = new EntityConfigurationBuilder()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .AddInstance<EntityConfiguration>(configuration)
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(IServiceProvider serviceProvider, EntityConfiguration configuration)
                    : base(serviceProvider, configuration)
                {
                    Assert.NotNull(serviceProvider);
                    Assert.NotNull(configuration);
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        public class InjectConfiguration
        {
            // This one is a bit strange because the context gets the configuration from the service provider
            // but doesn't get the service provider and so creates a new one for use internally. This works fine
            // although it would be much more common to inject both when using DI explicitly.
            [Fact]
            public void Can_register_configuration_with_DI_container_and_have_it_injected()
            {
                var configuration = new EntityConfigurationBuilder()
                    .UseInMemoryStore()
                    .BuildConfiguration();

                var serviceProvider = new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .AddTransient<BlogContext, BlogContext>()
                    .AddTransient<MyController, MyController>()
                    .AddInstance<EntityConfiguration>(configuration)
                    .BuildServiceProvider();

                serviceProvider.GetService<MyController>().Test();
            }

            private class MyController
            {
                private readonly BlogContext _context;

                public MyController(BlogContext context)
                {
                    Assert.NotNull(context);

                    _context = context;
                }

                public void Test()
                {
                    _context.Blogs.Add(new Blog { Id = 1, Name = "The Waffle Cart" });
                    _context.SaveChanges();

                    var blog = _context.Blogs.SingleOrDefault();

                    Assert.Equal(1, blog.Id);
                    Assert.Equal("The Waffle Cart", blog.Name);
                }
            }

            private class BlogContext : EntityContext
            {
                public BlogContext(EntityConfiguration configuration)
                    : base(configuration)
                {
                    Assert.NotNull(configuration);
                }

                public EntitySet<Blog> Blogs { get; set; }
            }
        }

        private class Blog
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static string GetString(string stringName)
        {
            var strings = typeof(EntityContext).Assembly.GetType(typeof(EntityContext).Namespace + ".Strings");
            return (string)strings.GetTypeInfo().GetDeclaredMethods(stringName).Single().Invoke(null, null);
        }
    }
}
