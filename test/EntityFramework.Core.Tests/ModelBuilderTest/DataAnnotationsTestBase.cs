// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class DataAnnotationsTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void NotMappedAttribute_removes_ambiguity_in_conventional_relationship_building()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Book>();

                Assert.Contains("Details", model.GetEntityType(typeof(Book)).Navigations.Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.GetEntityType(typeof(BookDetails)).Navigations.Select(nav => nav.Name));
                Assert.DoesNotContain("Book", model.GetEntityType(typeof(BookDetails)).Navigations.Select(nav => nav.Name));
            }

            [Fact]
            public virtual void InversePropertyAttribute_removes_ambiguity_in_conventional_relationalship_building()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Book>();

                Assert.Contains("Book", model.GetEntityType(typeof(BookLabel)).Navigations.Select(nav => nav.Name));
                Assert.Equal("Label", model.GetEntityType(typeof(BookLabel)).FindNavigation("Book").ForeignKey.PrincipalToDependent.Name);

                Assert.Contains("AlternateLabel", model.GetEntityType(typeof(Book)).Navigations.Select(nav => nav.Name));
                Assert.Null(model.GetEntityType(typeof(Book)).FindNavigation("AlternateLabel").ForeignKey.PrincipalToDependent);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_property_on_both_side()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Post>();

                Assert.Null(model.GetEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
                Assert.Equal("AuthorCode", model.GetEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

                Assert.Null(model.GetEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostNumber", model.GetEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_does_not_set_foreign_key_properties_when_applied_on_navigations_and_names_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Post>();

                Assert.Equal("PostId", model.GetEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_does_not_set_foreign_key_properties_when_names_on_property_and_navigation_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Author>();

                Assert.Equal("AuthorId", model.GetEntityType(typeof(AuthorDetails)).FindNavigation("Author").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_does_not_set_foreign_key_properties_when_applied_on_properties_for_composite_foreign_key()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<PostDetails>();

                Assert.Equal("AuthorId", model.GetEntityType(typeof(PostDetails)).FindNavigation("Author").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_does_not_set_foreign_key_properties_when_name_is_incorrect_on_navigation()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<PostDetails>();

                Assert.Equal("PostDetailsId", model.GetEntityType(typeof(AuthorDetails)).FindNavigation("PostDetails").ForeignKey.Properties.First().Name);
            }
        }
    }
}
