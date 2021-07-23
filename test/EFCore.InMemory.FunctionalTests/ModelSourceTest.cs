// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class ModelSourceTest
    {
        [ConditionalFact] // Issue #2992
        public void Can_customize_ModelBuilder()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelCustomizer, MyModelCustomizer>()
                .BuildServiceProvider();

            using var context = new JustSomeContext(serviceProvider);
            var model = context.Model;
            Assert.Equal("Us!", model["AllYourModelAreBelongTo"]);
            Assert.Equal("Us!", model.GetEntityTypes().Single(e => e.DisplayName() == "Base")["AllYourBaseAreBelongTo"]);
            Assert.Contains("Peak", model.GetEntityTypes().Select(e => e.DisplayName()));
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
