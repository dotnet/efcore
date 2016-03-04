// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class ModelSourceTest
    {
        [Fact] // Issue #943
        public void Can_replace_ModelSource_without_access_to_internals()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddSingleton<InMemoryModelSource, MyModelSource>()
                .BuildServiceProvider();

            using (var context = new JustSomeContext(serviceProvider))
            {
                var model = context.Model;

                Assert.Equal("Us!", model["AllYourModelAreBelongTo"]);
                Assert.Equal("Us!", model.GetEntityTypes().Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
                Assert.Contains("Peak", model.GetEntityTypes().Select(e => e.DisplayName()));
            }
        }

        [Fact] // Issue #2992
        public void Can_customize_ModelBuilder()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
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
            public override void Customize(ModelBuilder modelBuilder, DbContext dbContext)
            {
                base.Customize(modelBuilder, dbContext);
                modelBuilder.Model["AllYourModelAreBelongTo"] = "Us!";
            }
        }

        private class MyModelSource : InMemoryModelSource
        {
            public MyModelSource(IDbSetFinder setFinder, ICoreConventionSetBuilder coreConventionSetBuilder, IModelCustomizer modelCustomizer, IModelCacheKeyFactory modelCacheKeyFactory)
                : base(setFinder, coreConventionSetBuilder, modelCustomizer, modelCacheKeyFactory)
            {
            }

            protected override IModel CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
            {
                var model = base.CreateModel(context, conventionSetBuilder, validator) as Model;

                model["AllYourModelAreBelongTo"] = "Us!";

                return model;
            }
        }

        private class JustSomeContext : DbContext
        {
            public JustSomeContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            public DbSet<Peak> Peaks { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Base>().HasAnnotation("AllYourBaseAreBelongTo", "Us!");

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase();
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
