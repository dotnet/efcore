// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderOtherTest
    {
        [ConditionalFact]
        public virtual void HasOne_with_just_string_navigation_for_non_CLR_property_throws()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<Dr>().HasOne("Snoop");
                }))
            {
                Assert.Equal(
                    CoreStrings.NoClrNavigation("Snoop", nameof(Dr)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public virtual void HasMany_with_just_string_navigation_for_non_CLR_property_throws()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<Dr>().HasMany("Snoop");
                }))
            {
                Assert.Equal(
                    CoreStrings.NoClrNavigation("Snoop", nameof(Dr)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public virtual void HasMany_with_a_non_collection_just_string_navigation_CLR_property_throws()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<Dr>().HasMany("Dre");
                }))
            {
                Assert.Equal(
                    CoreStrings.NavigationCollectionWrongClrType("Dre", nameof(Dr), nameof(Dre), "T"),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public virtual void HasMany_with_a_collection_navigation_CLR_property_to_derived_type_throws()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<Dr>().HasMany<Dre>(d => d.Jrs);
                }))
            {
                Assert.Equal(
                    CoreStrings.NavigationCollectionWrongClrType(nameof(Dr.Jrs), nameof(Dr), "ICollection<DreJr>", nameof(Dre)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public virtual void OwnsOne_HasOne_with_just_string_navigation_for_non_CLR_property_throws()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<Dr>().OwnsOne(e => e.Dre).HasOne("Snoop");
                }))
            {
                Assert.Equal(
                    CoreStrings.NoClrNavigation("Snoop", nameof(Dre)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        protected class Dr
        {
            public int Id { get; set; }

            public Dre Dre { get; set; }

            public ICollection<DreJr> Jrs { get; set; }
        }

        protected class Dre
        {
        }

        protected class DreJr : Dre
        {
        }

        [ConditionalFact] //Issue#13108
        public virtual void HasForeignKey_infers_type_for_shadow_property_when_not_specified()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<ComplexCaseChild13108>(
                        e =>
                        {
                            e.HasKey(c => c.Key);
                            e.Property("ParentKey");
                            e.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey("ParentKey");
                        });

                    b.Entity<ComplexCaseParent13108>().HasKey(c => c.Key);
                }))
            {
                var model = (IConventionModel)context.Model;
                var property = model
                    .FindEntityType(typeof(ComplexCaseChild13108)).GetProperties().Single(p => p.Name == "ParentKey");
                Assert.Equal(typeof(int), property.ClrType);
                Assert.Equal(ConfigurationSource.Explicit, property.GetTypeConfigurationSource());
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

        [ConditionalFact] //Issue#12617
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

        [ConditionalFact] //Issue#13300
        public virtual void Explicitly_set_shadow_FK_name_is_preserved_with_HasPrincipalKey()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<User13300>(
                        m =>
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
            private readonly string _email = string.Empty;
            private readonly List<Profile13300> _profiles = new List<Profile13300>();
        }

        protected class Profile13300
        {
            public Guid Id { get; set; }
            public User13300 User { get; set; }
        }

        [ConditionalFact]
        public virtual void Attribute_set_shadow_FK_name_is_preserved_with_HasPrincipalKey()
        {
            using (var context = new CustomModelBuildingContext(
                Configure(),
                b =>
                {
                    b.Entity<User13694>(
                        m =>
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
            private readonly string _email = string.Empty;
            private readonly List<Profile13694> _profiles = new List<Profile13694>();
        }

        protected class Profile13694
        {
            public Guid Id { get; set; }

            [ForeignKey("Email")]
            public User13694 User { get; set; }
        }

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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

        [ConditionalFact]
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
                : base(options)
            {
                _builder = builder;
            }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => _builder(modelBuilder);
        }

        protected virtual void RunThrowDifferPipeline(DbContext context)
        {
        }

        protected class TestModelCacheKeyFactory : IModelCacheKeyFactory
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
                .UseInternalServiceProvider(
                    InMemoryFixture.BuildServiceProvider(
                        new ServiceCollection()
                            .AddSingleton<IModelCacheKeyFactory, TestModelCacheKeyFactory>()))
                .UseInMemoryDatabase(nameof(CustomModelBuildingContext))
                .Options;
    }
}
