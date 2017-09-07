// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ModelSourceTest
    {
        [Fact]
        public void Can_replace_default_model_customizer_with_derived()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelCustomizer, MyModelCustomizer>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;
                Assert.Equal("Us!", model["AllYourModelAreBelongTo"]);
                Assert.Equal("Us!", model.GetEntityTypes().Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
                Assert.Contains("Peak", model.GetEntityTypes().Select(e => e.DisplayName()));
            }
        }

        private class MyModelCustomizer : ModelCustomizer
        {
            public MyModelCustomizer(ModelCustomizerDependencies dependencies)
                : base(dependencies)
            {
            }

            public override void Customize(ModelBuilder modelBuilder, DbContext dbContext)
            {
                base.Customize(modelBuilder, dbContext);
                modelBuilder.Model["AllYourModelAreBelongTo"] = "Us!";
            }
        }

        [Fact]
        public void Can_replace_default_model_customizer()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelCustomizer, MyMinimalCustomizer>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;
                Assert.Equal("NotUs!", model["AllYourModelAreBelongTo"]);
                Assert.Empty(model.GetEntityTypes());
            }
        }

        [Fact]
        public void Can_customize_ModelBuilder_without_replacing_default()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IAdditionalModelCustomizer, MyMinimalCustomizer>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;
                Assert.Equal("NotUs!", model["AllYourModelAreBelongTo"]);
                Assert.Equal("Us!", model.GetEntityTypes().Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
                Assert.Contains("Peak", model.GetEntityTypes().Select(e => e.DisplayName()));
            }
        }

        private class MyMinimalCustomizer : IAdditionalModelCustomizer
        {
            public void Customize(ModelBuilder modelBuilder, DbContext dbContext)
            {
                modelBuilder.Model["AllYourModelAreBelongTo"] = "NotUs!";
            }
        }

        [Fact]
        public void Can_change_ModelBuilder_customization_order()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelCustomizer, MyModelCustomizer>()
                .AddSingleton<IAdditionalModelCustomizer, MyMinimalCustomizer>()
                .AddSingleton<IModelCustomizerCollection, ReverseModelCustomizerCollection>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;
                Assert.Equal("Us!", model["AllYourModelAreBelongTo"]);
                Assert.Equal("Us!", model.GetEntityTypes().Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
                Assert.Contains("Peak", model.GetEntityTypes().Select(e => e.DisplayName()));
            }
        }

        [Fact]
        public void Can_remove_default_customizer()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelCustomizerCollection, EmptyModelCustomizerCollection>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;
                Assert.Null(model["AllYourModelAreBelongTo"]);
                Assert.Empty(model.GetEntityTypes());
            }
        }

        private class EmptyModelCustomizerCollection : IModelCustomizerCollection
        {
            public IEnumerable<IModelCustomizer> Items { get; } = new IModelCustomizer[0];
        }

        private class ReverseModelCustomizerCollection : ModelCustomizerCollection
        {
            public ReverseModelCustomizerCollection(ModelCustomizerCollectionDependencies dependencies)
                : base(dependencies)
            {
                Customizers.Reverse();
            }
        }

        private class JustSomeContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public JustSomeContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Peak> Peaks { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Base>().HasAnnotation("AllYourBaseAreBelongTo", "Us!");

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(nameof(JustSomeContext))
                    .UseInternalServiceProvider(_serviceProvider);
        }

        private class Base
        {
            public int Id { get; set; }
        }

        private class Peak
        {
            public int Id { get; set; }
        }
    }
}
