// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Tests
{
    public abstract partial class ModelBuilderTest
    {
        public abstract class DataAnnotationsTestBase : ModelBuilderTestBase
        {
            [Fact]
            public virtual void NotMappedAttribute_removes_ambiguity_in_conventional_relationship_building()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Book>();

                Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
                Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
            }

            [Fact]
            public virtual void NotMappedAttribute_removes_ambiguity_in_conventional_relationship_building_with_base()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<BookDetailsBase>();
                modelBuilder.Entity<Book>();

                Assert.Same(model.FindEntityType(typeof(BookDetailsBase)), model.FindEntityType(typeof(BookDetails)).BaseType);
                Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetailsBase)).GetNavigations().Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
                Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));

                modelBuilder.Entity<BookDetails>().HasBaseType(null);

                Assert.Same(model.FindEntityType(typeof(BookDetails)),
                    model.FindEntityType(typeof(Book)).GetNavigations().Single(n => n.Name == "Details").ForeignKey.DeclaringEntityType);
                Assert.Contains("Details", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetailsBase)).GetNavigations().Select(nav => nav.Name));
                Assert.Contains("AnotherBook", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
                Assert.DoesNotContain("Book", model.FindEntityType(typeof(BookDetails)).GetNavigations().Select(nav => nav.Name));
            }

            [Fact]
            public virtual void InversePropertyAttribute_removes_ambiguity_in_conventional_relationalship_building()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Book>();

                Assert.Equal("Label",
                    model.FindEntityType(typeof(BookLabel)).FindNavigation("Book").FindInverse().Name);

                Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation("AlternateLabel").FindInverse());
            }

            [Fact]
            public virtual void InversePropertyAttribute_removes_ambiguity_in_conventional_relationalship_building_with_base()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<SpecialBookLabel>();

                Assert.Same(model.FindEntityType(typeof(BookLabel)), model.FindEntityType(typeof(SpecialBookLabel)).BaseType);
                Assert.Contains("Book", model.FindEntityType(typeof(BookLabel)).GetNavigations().Select(nav => nav.Name));
                Assert.Equal("Label", model.FindEntityType(typeof(BookLabel)).FindNavigation("Book").ForeignKey.PrincipalToDependent.Name);
                Assert.Contains("AlternateLabel", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation("AlternateLabel").ForeignKey.PrincipalToDependent);

                modelBuilder.Entity<SpecialBookLabel>().HasBaseType(null);

                Assert.Null(model.FindEntityType(typeof(SpecialBookLabel)).GetNavigations().Single(n => n.Name == "Book").FindInverse());
                Assert.Contains("Book", model.FindEntityType(typeof(BookLabel)).GetNavigations().Select(nav => nav.Name));
                Assert.Equal("Label", model.FindEntityType(typeof(BookLabel)).FindNavigation("Book").ForeignKey.PrincipalToDependent.Name);
                Assert.Contains("AlternateLabel", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation("AlternateLabel").ForeignKey.PrincipalToDependent);
            }

            // TODO: Support base type ignore
            //[Fact]
            public virtual void InversePropertyAttribute_removes_ambiguity_in_conventional_relationalship_building_with_base_ignored()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<SpecialBookLabel>().HasBaseType(null);
                modelBuilder.Ignore<BookLabel>();

                Assert.Null(model.FindEntityType(typeof(BookLabel)));
                Assert.Contains("Book", model.FindEntityType(typeof(SpecialBookLabel)).GetNavigations().Select(nav => nav.Name));
                Assert.Equal("Label", model.FindEntityType(typeof(SpecialBookLabel)).FindNavigation("Book").ForeignKey.PrincipalToDependent.Name);
                Assert.Contains("AlternateLabel", model.FindEntityType(typeof(Book)).GetNavigations().Select(nav => nav.Name));
                Assert.Null(model.FindEntityType(typeof(Book)).FindNavigation("AlternateLabel").ForeignKey.PrincipalToDependent);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_property_on_both_side()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Post>();

                Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostDetailsId", model.FindEntityType(typeof(Post)).FindNavigation("PostDetails").ForeignKey.Properties.First().Name);

                Assert.Null(model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostId", model.FindEntityType(typeof(PostDetails)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigations_on_both_side_and_values_do_not_match()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Post>();

                Assert.Null(model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.PrincipalToDependent);
                Assert.Equal("AuthorId", model.FindEntityType(typeof(Post)).FindNavigation("Author").ForeignKey.Properties.First().Name);

                Assert.Null(model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.PrincipalToDependent);
                Assert.Equal("PostId", model.FindEntityType(typeof(Author)).FindNavigation("Post").ForeignKey.Properties.First().Name);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_creates_two_relationships_if_applied_on_navigation_and_property_on_different_side_and_values_do_not_match()
            {
                var modelBuilder = CreateModelBuilder();
                var model = modelBuilder.Model;
                modelBuilder.Entity<Author>();

                var authorDetails = model.FindEntityType(typeof(AuthorDetails));
                var firstFk = authorDetails.FindNavigation(nameof(AuthorDetails.Author)).ForeignKey;
                Assert.Equal(typeof(AuthorDetails), firstFk.DeclaringEntityType.ClrType);
                Assert.Equal("AuthorId", firstFk.Properties.First().Name);

                var author = model.FindEntityType(typeof(Author));
                var secondFk = author.FindNavigation(nameof(Author.AuthorDetails)).ForeignKey;
                Assert.Equal(typeof(Author), secondFk.DeclaringEntityType.ClrType);
                Assert.Equal("AuthorDetailsIdByAttribute", secondFk.Properties.First().Name);

                AssertEqual(new[] { "AuthorId", "Id" }, authorDetails.GetProperties().Select(p => p.Name));
                AssertEqual(new[] { "AuthorDetailsIdByAttribute", "Id", "PostId" }, author.GetProperties().Select(p => p.Name));
            }

            [Fact]
            public virtual void ForeignKeyAttribute_throws_if_applied_on_property_on_both_side_but_navigations_are_connected_by_inverse_property()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(CoreStrings.InvalidRelationshipUsingDataAnnotations("B", typeof(A).FullName, "A", typeof(B).FullName),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<A>()).Message);
            }

            [Fact]
            public virtual void ForeignKeyAttribute_throws_if_applied_on_both_navigations_connected_by_inverse_property_but_values_do_not_match()
            {
                var modelBuilder = CreateModelBuilder();

                Assert.Equal(CoreStrings.InvalidRelationshipUsingDataAnnotations("C", typeof(D).FullName, "D", typeof(C).FullName),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<D>()).Message);
            }
        }
    }
}
