// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class LazyLoadingProxyTests
    {
        [ConditionalFact]
        public void Throws_if_sealed_class()
        {
            using var context = new LazyContext<LazySealedEntity>();
            Assert.Equal(
                ProxiesStrings.ItsASeal(nameof(LazySealedEntity)),
                Assert.Throws<InvalidOperationException>(
                    () => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_if_non_virtual_navigation_to_non_owned_type()
        {
            using var context = new LazyContext<LazyNonVirtualNavEntity>();
            Assert.Equal(
                ProxiesStrings.NonVirtualProperty(nameof(LazyNonVirtualNavEntity.SelfRef), nameof(LazyNonVirtualNavEntity)),
                Assert.Throws<InvalidOperationException>(
                    () => context.Model).Message);
        }

        [ConditionalFact]
        public void Does_not_throw_if_non_virtual_navigation_to_owned_type()
        {
            using var context = new LazyContext<LazyNonVirtualOwnedNavEntity>();
            var model = context.Model;
        }

        [ConditionalFact]
        public void Throws_if_no_field_found()
        {
            using var context = new LazyContext<LazyHiddenFieldEntity>();
            Assert.Equal(
                CoreStrings.NoBackingFieldLazyLoading(nameof(LazyHiddenFieldEntity.SelfRef), nameof(LazyHiddenFieldEntity)),
                Assert.Throws<InvalidOperationException>(
                    () => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_when_context_is_disposed()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddEntityFrameworkProxies()
                .AddDbContext<JammieDodgerContext>(
                    (p, b) =>
                        b.UseInMemoryDatabase("Jammie")
                            .UseInternalServiceProvider(p)
                            .UseLazyLoadingProxies())
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<JammieDodgerContext>();
                context.Add(new Phone());
                context.SaveChanges();
            }

            Phone phone;
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<JammieDodgerContext>();
                phone = context.Set<Phone>().Single();
            }

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.LazyLoadOnDisposedContextWarning.ToString(),
                    CoreResources.LogLazyLoadOnDisposedContext(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage("Texts", "PhoneProxy"),
                    "CoreEventId.LazyLoadOnDisposedContextWarning"),
                Assert.Throws<InvalidOperationException>(
                    () => phone.Texts).Message);
        }

        private class LazyContext<TEntity> : TestContext<TEntity>
            where TEntity : class
        {
            public LazyContext()
                : base(dbName: "LazyLoadingContext", useLazyLoading: true, useChangeDetection: false)
            {
            }
        }

        public sealed class LazySealedEntity
        {
            public int Id { get; set; }
        }

        public class LazyNonVirtualNavEntity
        {
            public int Id { get; set; }

            public LazyNonVirtualNavEntity SelfRef { get; set; }
        }

        public class LazyNonVirtualOwnedNavEntity
        {
            public int Id { get; set; }

            public OwnedNavEntity NavigationToOwned { get; set; }
        }

        [Owned]
        public class OwnedNavEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public LazyNonVirtualOwnedNavEntity Owner { get; set; }
        }

        public class LazyHiddenFieldEntity
        {
            private LazyHiddenFieldEntity _hiddenBackingField;

            public int Id { get; set; }

            // ReSharper disable once ConvertToAutoProperty
            public virtual LazyHiddenFieldEntity SelfRef
            {
                get => _hiddenBackingField;
                set => _hiddenBackingField = value;
            }
        }

        private class JammieDodgerContext : DbContext
        {
            public JammieDodgerContext(DbContextOptions options)
                : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Phone>();
        }

        public class Phone
        {
            public int Id { get; set; }
            public virtual ICollection<Text> Texts { get; set; }
        }

        public class Text
        {
            public int Id { get; set; }
        }
    }
}
