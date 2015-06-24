// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Internal;
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
                
                Assert.Equal("Label",
                    model.GetEntityType(typeof(BookLabel)).FindNavigation("Book").FindInverse().Name);
                
                Assert.Null(model.GetEntityType(typeof(Book)).FindNavigation("AlternateLabel").FindInverse());
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_property_on_both_side()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Post>();

                Assert.Null(model.GetEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostDetailsId", model.GetEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.Properties.First().Name);

                Assert.Null(model.GetEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostId", model.GetEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigations_on_both_side_and_values_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Post>();

                Assert.Null(model.GetEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
                Assert.Equal("AuthorId", model.GetEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

                Assert.Null(model.GetEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostId", model.GetEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigation_and_property_on_different_side_and_values_do_not_match()
            {
                var model = new Model();
                var modelBuilder = CreateModelBuilder(model);
                modelBuilder.Entity<Author>();

                Assert.Null(model.GetEntityType(typeof(AuthorDetails)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
                Assert.Equal("AuthorId", model.GetEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

                Assert.Null(model.GetEntityType(typeof(Author)).FindNavigation("AuthorDetails").ForeignKey.PrincipalToDependent);
                Assert.Equal("AuthorDetailsId", model.GetEntityType(typeof(Author)).FindNavigation("AuthorDetails").ForeignKey.Properties.First().Name);
            }


            [Fact]
            public virtual void ForeignKeyAttribute_throws_if_applied_on_property_on_both_side_but_navigations_are_connected_by_inverse_property()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(Strings.InvalidRelationshipUsingDataAnnotations("B", typeof(A).FullName, "A", typeof(B).FullName),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<A>()).Message);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_throws_if_applied_on_both_navigations_connected_by_inverse_property_but_values_do_not_match()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(Strings.InvalidRelationshipUsingDataAnnotations("C", typeof(D).FullName, "D", typeof(C).FullName),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<D>()).Message);
            }
        }
    }
}
