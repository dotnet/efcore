// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadingProxyTests
    {
        [Fact]
        public void Materialization_uses_parameterless_constructor()
        {
            using (var context = new NeweyContext(nameof(Materialization_uses_parameterless_constructor)))
            {
                context.Add(new March82GGtp());
                context.SaveChanges();
            }

            using (var context = new NeweyContext(nameof(Materialization_uses_parameterless_constructor)))
            {
                Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
            }
        }

        [Fact]
        public void Materialization_uses_parameterized_constructor()
        {
            using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor)))
            {
                context.Add(new March881(77, "Leyton House"));
                context.SaveChanges();
            }

            using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor)))
            {
                var proxy = context.Set<March881>().Single();

                Assert.Same(typeof(March881), proxy.GetType().BaseType);
                Assert.Equal(77, proxy.Id);
                Assert.Equal("Leyton House", proxy.Sponsor);
            }
        }

        [Fact]
        public void Materialization_uses_parameterized_constructor_taking_context()
        {
            using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor_taking_context)))
            {
                context.Add(new WilliamsFw14(context, 6, "Canon"));
                context.SaveChanges();
            }

            using (var context = new NeweyContext(nameof(Materialization_uses_parameterized_constructor_taking_context)))
            {
                var proxy = context.Set<WilliamsFw14>().Single();

                Assert.Same(typeof(WilliamsFw14), proxy.GetType().BaseType);
                Assert.Same(context, proxy.Context);
                Assert.Equal(6, proxy.Id);
                Assert.Equal("Canon", proxy.Sponsor);
            }
        }

        [Fact]
        public void CreateProxy_uses_parameterless_constructor()
        {
            using (var context = new NeweyContext())
            {
                Assert.Same(typeof(March82GGtp), context.CreateProxy<March82GGtp>().GetType().BaseType);
            }
        }

        [Fact]
        public void CreateProxy_uses_parameterized_constructor()
        {
            using (var context = new NeweyContext())
            {
                var proxy = context.CreateProxy<March881>(77, "Leyton House");

                Assert.Same(typeof(March881), proxy.GetType().BaseType);
                Assert.Equal(77, proxy.Id);
                Assert.Equal("Leyton House", proxy.Sponsor);
            }
        }

        [Fact]
        public void CreateProxy_uses_parameterized_constructor_taking_context()
        {
            using (var context = new NeweyContext())
            {
                var proxy = context.CreateProxy<WilliamsFw14>(context, 6, "Canon");

                Assert.Same(typeof(WilliamsFw14), proxy.GetType().BaseType);
                Assert.Same(context, proxy.Context);
                Assert.Equal(6, proxy.Id);
                Assert.Equal("Canon", proxy.Sponsor);
            }
        }

        [Fact]
        public void Proxies_only_created_if_Use_called()
        {
            using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), false))
            {
                context.Add(new March82GGtp());
                context.SaveChanges();
            }

            using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called), false))
            {
                Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType());
            }

            using (var context = new NeweyContext(nameof(Proxies_only_created_if_Use_called)))
            {
                Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
            }
        }

        [Fact]
        public void Proxy_services_must_be_available()
        {
            var withoutProxies = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var withProxies = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddEntityFrameworkProxies()
                .BuildServiceProvider();

            using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available), false))
            {
                context.Add(new March82GGtp());
                context.SaveChanges();
            }

            using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available), false))
            {
                Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType());
            }

            using (var context = new NeweyContext(withProxies, nameof(Proxy_services_must_be_available)))
            {
                Assert.Same(typeof(March82GGtp), context.Set<March82GGtp>().Single().GetType().BaseType);
            }

            using (var context = new NeweyContext(withoutProxies, nameof(Proxy_services_must_be_available)))
            {
                Assert.Equal(
                    ProxiesStrings.ProxyServicesMissing,
                    Assert.Throws<InvalidOperationException>(
                        () => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_if_sealed_class()
        {
            using (var context = new NeweyContextN1())
            {
                Assert.Equal(
                    ProxiesStrings.ItsASeal(nameof(McLarenMp418)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_if_non_virtual_navigation()
        {
            using (var context = new NeweyContextN2())
            {
                Assert.Equal(
                    ProxiesStrings.NonVirtualNavigation(nameof(McLarenMp419.SelfRef), nameof(McLarenMp419)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_if_no_field_found()
        {
            using (var context = new NeweyContextN3())
            {
                Assert.Equal(
                    CoreStrings.NoBackingFieldLazyLoading(nameof(MarchCg901.SelfRef), nameof(MarchCg901)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_if_type_not_available_to_Castle()
        {
            using (var context = new NeweyContextN4())
            {
                Assert.Throws<GeneratorException>(() => context.CreateProxy<McLarenMp421>());
            }
        }

        [Fact]
        public void Throws_if_constructor_not_available_to_Castle()
        {
            using (var context = new NeweyContextN5())
            {
                Assert.Throws<InvalidProxyConstructorArgumentsException>(() => context.CreateProxy<RedBullRb3>());
            }
        }

        [Fact]
        public void CreateProxy_throws_if_constructor_args_do_not_match()
        {
            using (var context = new NeweyContext())
            {
                Assert.Throws<InvalidProxyConstructorArgumentsException>(() => context.CreateProxy<March881>(77, 88));
            }
        }

        [Fact]
        public void CreateProxy_throws_if_wrong_number_of_constructor_args()
        {
            using (var context = new NeweyContext())
            {
                Assert.Throws<InvalidProxyConstructorArgumentsException>(() => context.CreateProxy<March881>(77, 88, 99));
            }
        }

        [Fact]
        public void Throws_if_create_proxy_for_non_mapped_type()
        {
            using (var context = new NeweyContextN4())
            {
                Assert.Equal(
                    CoreStrings.EntityTypeNotFound(nameof(March82GGtp)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.CreateProxy<March82GGtp>()).Message);
            }
        }

        [Fact]
        public void Throws_if_create_proxy_when_proxies_not_used()
        {
            using (var context = new NeweyContextN6())
            {
                Assert.Equal(
                    ProxiesStrings.ProxiesNotEnabled(nameof(RedBullRb3)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.CreateProxy<RedBullRb3>()).Message);
            }
        }

        [Fact]
        public void Throws_if_create_proxy_when_proxies_not_enabled()
        {
            using (var context = new NeweyContextN7())
            {
                Assert.Equal(
                    ProxiesStrings.ProxiesNotEnabled(nameof(RedBullRb3)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.CreateProxy<RedBullRb3>()).Message);
            }
        }

        public class March82GGtp
        {
            public int Id { get; set; }
        }

        public class March881
        {
            public March881(int id, string sponsor)
            {
                Id = id;
                Sponsor = sponsor;
            }

            public int Id { get; }
            public string Sponsor { get; }
        }

        public class WilliamsFw14
        {
            public WilliamsFw14(DbContext context, int id, string sponsor)
            {
                Context = context;
                Id = id;
                Sponsor = sponsor;
            }

            public DbContext Context { get; }
            public int Id { get; }
            public string Sponsor { get; }
        }

        private class NeweyContext : DbContext
        {
            private readonly IServiceProvider _internalServiceProvider;
            private static readonly InMemoryDatabaseRoot _dbRoot = new InMemoryDatabaseRoot();
            private readonly bool _useProxies;
            private readonly string _dbName;

            public NeweyContext(string dbName = null, bool useProxies = true)
            {
                _dbName = dbName;
                _useProxies = useProxies;
            }

            public NeweyContext(IServiceProvider internalServiceProvider, string dbName = null, bool useProxies = true)
            : this(dbName, useProxies)
            {
                _internalServiceProvider = internalServiceProvider;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (_useProxies)
                {
                    optionsBuilder.UseLazyLoadingProxies();
                }

                if (_internalServiceProvider != null)
                {
                    optionsBuilder.UseInternalServiceProvider(_internalServiceProvider);
                }

                optionsBuilder.UseInMemoryDatabase(_dbName ?? nameof(NeweyContext), _dbRoot);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<March82GGtp>();

                modelBuilder.Entity<March881>(
                    b =>
                    {
                        b.Property(e => e.Id);
                        b.Property(e => e.Sponsor);
                    });

                modelBuilder.Entity<WilliamsFw14>(
                    b =>
                    {
                        b.Property(e => e.Id);
                        b.Property(e => e.Sponsor);
                    });
            }
        }

        public sealed class McLarenMp418
        {
            public int Id { get; set; }
        }

        private class NeweyContextN : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        private class NeweyContextN1 : NeweyContextN
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<McLarenMp418>();
        }

        public class McLarenMp419
        {
            public int Id { get; set; }

            public McLarenMp419 SelfRef { get; set; }
        }

        private class NeweyContextN2 : NeweyContextN
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<McLarenMp419>();
        }

        public class MarchCg901
        {
            private MarchCg901 _hiddenBackingField;

            public int Id { get; set; }

            // ReSharper disable once ConvertToAutoProperty
            public virtual MarchCg901 SelfRef
            {
                get => _hiddenBackingField;
                set => _hiddenBackingField = value;
            }
        }

        private class NeweyContextN3 : NeweyContextN
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<MarchCg901>();
        }

        internal class McLarenMp421
        {
            public int Id { get; set; }
        }

        private class NeweyContextN4 : NeweyContextN
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<McLarenMp421>();
        }

        public class RedBullRb3
        {
            internal RedBullRb3()
            {
            }

            public int Id { get; set; }
        }

        private class NeweyContextN5 : NeweyContextN
        {
            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<RedBullRb3>();
        }

        private class NeweyContextN6 : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<March82GGtp>();
        }

        private class NeweyContextN7 : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLazyLoadingProxies(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<March82GGtp>();
        }
    }
}
