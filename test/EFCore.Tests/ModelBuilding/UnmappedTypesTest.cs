// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class UnmappedTypesTest
    {
        [Fact]
        protected virtual void Mapping_throws_for_non_ignored_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<OneDee>()))
            {
                Assert.Equal(
                    CoreStrings.PropertyNotAdded(
                        typeof(OneDee).ShortDisplayName(), "One", typeof(int[]).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        protected virtual void Mapping_ignores_ignored_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<OneDee>().Ignore(e => e.One)))
            {
                Assert.Null(context.Model.FindEntityType(typeof(OneDee)).FindProperty("One"));

                RunThrowDifferPipeline(context);
            }
        }

        [Fact]
        protected virtual void Mapping_throws_for_non_ignored_two_dimensional_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<TwoDee>()))
            {
                Assert.Equal(
                    CoreStrings.PropertyNotAdded(
                        typeof(TwoDee).ShortDisplayName(), "Two", typeof(int[,]).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        protected virtual void Mapping_ignores_ignored_two_dimensional_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<TwoDee>().Ignore(e => e.Two)))
            {
                Assert.Null(context.Model.FindEntityType(typeof(TwoDee)).FindProperty("Two"));

                RunThrowDifferPipeline(context);
            }
        }

        [Fact]
        protected virtual void Mapping_throws_for_non_ignored_three_dimensional_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<ThreeDee>()))
            {
                Assert.Equal(
                    CoreStrings.PropertyNotAdded(
                        typeof(ThreeDee).ShortDisplayName(), "Three", typeof(int[,,]).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        protected virtual void Mapping_ignores_ignored_three_dimensional_array()
        {
            using (var context = new TwoDeeContext(
                Configure(),
                b => b.Entity<ThreeDee>().Ignore(e => e.Three)))
            {
                Assert.Null(context.Model.FindEntityType(typeof(ThreeDee)).FindProperty("Three"));

                RunThrowDifferPipeline(context);
            }
        }

        protected class TwoDeeContext : DbContext
        {
            private readonly Action<ModelBuilder> _builder;

            public TwoDeeContext(DbContextOptions options, Action<ModelBuilder> builder)
                : base(
                    new DbContextOptionsBuilder(options)
                        .ReplaceService<IModelCacheKeyFactory, TestModelCacheKeyFactory>()
                        .Options)
            {
                _builder = builder;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => _builder(modelBuilder);
        }

        protected virtual void RunThrowDifferPipeline(DbContext context)
        {
        }

        private class TestModelCacheKeyFactory : IModelCacheKeyFactory
        {
            public object Create(DbContext context) => new object();
        }

        protected class OneDee
        {
            public int Id { get; set; }

            public int[] One { get; set; }
        }

        protected class TwoDee
        {
            public int Id { get; set; }

            public int[,] Two { get; set; }
        }

        protected class ThreeDee
        {
            public int Id { get; set; }

            public int[,,] Three { get; set; }
        }

        protected virtual DbContextOptions Configure()
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase(nameof(TwoDeeContext))
                .Options;
    }
}
