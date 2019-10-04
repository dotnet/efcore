// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderOtherTest
    {
        [Fact] //Issue#13108
        public virtual void HasForeignKey_infers_type_for_shadow_property_when_not_specified()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<ComplexCaseChild13108>(e =>
                    {
                        e.HasKey(c => c.Key);
                        e.Property("ParentKey");
                        e.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey("ParentKey");
                    });

                    b.Entity<ComplexCaseParent13108>().HasKey(c => c.Key);
                }))
            {
                var model = (Model)context.Model;
                Assert.Equal(ConfigurationSource.Convention,
                    model.FindEntityType(typeof(ComplexCaseChild13108))
                    .GetProperties().Where(p => p.Name == "ParentKey").Single()
                    .GetTypeConfigurationSource());
            }
        }

        protected class ComplexCaseChild13108
        {
            public int Key { get; set; }
            public string Id { get; set; }
            private int ParentKey { get; set; }
            public ComplexCaseParent13108 Parent { get; set; }
        }

        protected class ComplexCaseParent13108
        {
            public int Key { get; set; }
            public string Id { get; set; }
            public ICollection<ComplexCaseChild13108> Children { get; set; }
        }

        [Fact] //Issue#12617
        [UseCulture("de-DE")]
        public virtual void EntityType_name_is_stored_culture_invariantly()
        {
            using (var context = new CustomModelBuildingContext(
                 Configure(),
                 b =>
                 {
                     b.Entity<Entityß>();
                     b.Entity<Entityss>();
                 }))
            {
                Assert.Equal(2, context.Model.GetEntityTypes().Count());
                Assert.Equal(2, context.Model.FindEntityType(typeof(Entityss)).GetNavigations().Count());
            }
        }

        protected class Entityß
        {
            public int Id { get; set; }
        }

        protected class Entityss
        {
            public int Id { get; set; }
            public Entityß Navigationß { get; set; }
            public Entityß Navigationss { get; set; }
        }

        [Fact] //Issue#13300
        public virtual void Explicitly_set_shadow_FK_name_is_preserved_with_HasPrincipalKey()
        {
            using (var context = new CustomModelBuildingContext(
                 Configure(),
                 b =>
                 {
                     b.Entity<User13300>(m =>
                     {
                         m.Property("_email");

                         m.HasMany<Profile13300>("_profiles")
                             .WithOne("User")
                             .HasForeignKey("Email")
                             .HasPrincipalKey("_email");
                     });

                     b.Entity<Profile13300>().Property<string>("Email");
                 }))
            {
                var model = context.Model;

                var fk = model.FindEntityType(typeof(Profile13300)).GetForeignKeys().Single();
                Assert.Equal("_profiles", fk.PrincipalToDependent.Name);
                Assert.Equal("User", fk.DependentToPrincipal.Name);
                Assert.Equal("Email", fk.Properties[0].Name);
                Assert.Equal(typeof(string), fk.Properties[0].ClrType);
                Assert.Equal("_email", fk.PrincipalKey.Properties[0].Name);
            }
        }

        protected class User13300
        {
            public Guid Id { get; set; }
#pragma warning disable IDE0044 // Add readonly modifier
            private string _email = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier
            private readonly List<Profile13300> _profiles = new List<Profile13300>();
        }

        protected class Profile13300
        {
            public Guid Id { get; set; }
            public User13300 User { get; set; }
        }

        [Fact]
        public virtual void Attribute_set_shadow_FK_name_is_preserved_with_HasPrincipalKey()
        {
            using (var context = new CustomModelBuildingContext(
                 Configure(),
                 b =>
                 {
                     b.Entity<User13694>(m =>
                     {
                         m.Property("_email");

                         m.HasMany<Profile13694>("_profiles")
                             .WithOne("User")
                             .HasPrincipalKey("_email");
                     });

                     b.Entity<Profile13694>().Property<string>("Email");
                 }))
            {
                var model = context.Model;

                var fk = model.FindEntityType(typeof(Profile13694)).GetForeignKeys().Single();
                Assert.Equal("_profiles", fk.PrincipalToDependent.Name);
                Assert.Equal("User", fk.DependentToPrincipal.Name);
                Assert.Equal("Email", fk.Properties[0].Name);
                Assert.Equal(typeof(string), fk.Properties[0].ClrType);
                Assert.Equal("_email", fk.PrincipalKey.Properties[0].Name);
            }
        }

        protected class User13694
        {
            public Guid Id { get; set; }
#pragma warning disable IDE0044 // Add readonly modifier
            private string _email = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier
            private readonly List<Profile13694> _profiles = new List<Profile13694>();
        }

        protected class Profile13694
        {
            public Guid Id { get; set; }

            [ForeignKey("Email")]
            public User13694 User { get; set; }
        }

        [Fact]
        protected virtual void Mapping_throws_for_non_ignored_array()
        {
            using (var context = new CustomModelBuildingContext(
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
            using (var context = new CustomModelBuildingContext(
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
            using (var context = new CustomModelBuildingContext(
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
            using (var context = new CustomModelBuildingContext(
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
            using (var context = new CustomModelBuildingContext(
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
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b => b.Entity<ThreeDee>().Ignore(e => e.Three)))
            {
                Assert.Null(context.Model.FindEntityType(typeof(ThreeDee)).FindProperty("Three"));

                RunThrowDifferPipeline(context);
            }
        }

        protected class CustomModelBuildingContext : DbContext
        {
            private readonly Action<ModelBuilder> _builder;

            public CustomModelBuildingContext(DbContextOptions options, Action<ModelBuilder> builder)
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
                .UseInMemoryDatabase(nameof(CustomModelBuildingContext))
                .Options;
    }
}
