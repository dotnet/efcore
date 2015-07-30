// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Conventions
{
    public class NavigationAttributeConventionTest
    {
        #region NotMappedAttribute

        [Fact]
        public void NotMappedAttribute_overrides_configuration_from_convention_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Blog",
                "BlogDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { BlogDetails.BlogIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention,
                isUnique: true);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");

            new NotMappedNavigationAttributeConvention().Apply(principalEntityTypeBuilder);

            Assert.DoesNotContain(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");
        }

        [Fact]
        public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Blog",
                "BlogDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { BlogDetails.BlogIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Explicit,
                isUnique: true);


            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");

            new NotMappedNavigationAttributeConvention().Apply(principalEntityTypeBuilder);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");
        }

        [Fact]
        public void NotMappedAttribute_ignores_navigation_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<BlogDetails>();

            Assert.DoesNotContain(modelBuilder.Model.GetEntityType(typeof(Blog)).Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(BlogDetails)).Navigations, nav => nav.Name == "Blog");
        }

        #endregion

        #region RequiredAttribute

        [Fact]
        public void RequiredAttribute_overrides_configuration_from_convention_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Blog",
                "BlogDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { BlogDetails.BlogIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention,
                isUnique: true);

            var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation("Blog");

            relationshipBuilder.Required(false, ConfigurationSource.Convention);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = new RequiredNavigationAttributeConvention().Apply(relationshipBuilder, navigation);

            Assert.True(relationshipBuilder.Metadata.IsRequired);
            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");
        }

        [Fact]
        public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Blog), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Blog",
                "BlogDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { BlogDetails.BlogIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention,
                isUnique: true);

            var navigation = dependentEntityTypeBuilder.Metadata.FindNavigation("Blog");

            relationshipBuilder.Required(false, ConfigurationSource.Explicit);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = new RequiredNavigationAttributeConvention().Apply(relationshipBuilder, navigation);

            Assert.False(relationshipBuilder.Metadata.IsRequired);
            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Blog");
        }

        [Fact]
        public void RequiredAttribute_sets_is_required_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<BlogDetails>();

            Assert.True(entityTypeBuilder.Metadata.GetForeignKeys().Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Blog)).IsRequired);
            Assert.DoesNotContain(modelBuilder.Model.GetEntityType(typeof(Blog)).Navigations, nav => nav.Name == "BlogDetails");
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(BlogDetails)).Navigations, nav => nav.Name == "Blog");
        }

        [Fact]
        public void RequiredAttribute_does_not_set_is_required_with_conventional_builder_for_collection_navigation()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<Blog>();

            Assert.Null(entityTypeBuilder.Metadata.GetForeignKeys().Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Forum)).IsRequired);
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(Blog)).Navigations, nav => nav.Name == "Forum");
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(Forum)).Navigations, nav => nav.Name == "Blogs");
        }

        [Fact]
        public void RequiredAttribute_does_not_set_is_required_with_conventional_builder_for_navigation_to_dependent()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<Blog>();

            Assert.Null(modelBuilder.Model.GetEntityType(typeof(Blog)).GetForeignKeys().Single(fk => fk.PrincipalEntityType?.ClrType == typeof(Post)).IsRequired);
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(Blog)).Navigations, nav => nav.Name == "Post");
            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(Post)).Navigations, nav => nav.Name == "Blog");
        }

        #endregion

        #region InversePropertyAttribute

        [Fact]
        public void InversePropertyAttribute_overrides_configuration_from_convention_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<PostDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Post), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Post",
                "PostDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { PostDetails.PostIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention,
                isUnique: true);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "PostDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Post");
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "AnotherPost");

            new InversePropertyAttributeConvention().Apply(principalEntityTypeBuilder);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "PostDetails");
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Post");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "AnotherPost");
        }

        [Fact]
        public void InversePropertyAttribute_does_not_override_configuration_from_explicit_source()
        {
            var dependentEntityTypeBuilder = CreateInternalEntityTypeBuilder<PostDetails>();
            var principalEntityTypeBuilder = dependentEntityTypeBuilder.ModelBuilder.Entity(typeof(Post), ConfigurationSource.Convention);

            var relationshipBuilder = dependentEntityTypeBuilder.Relationship(
                principalEntityTypeBuilder,
                dependentEntityTypeBuilder,
                "Post",
                "PostDetails",
                dependentEntityTypeBuilder.GetOrCreateProperties(new List<PropertyInfo> { PostDetails.PostIdProperty }, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Explicit,
                isUnique: true);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "PostDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Post");
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "AnotherPost");

            new InversePropertyAttributeConvention().Apply(principalEntityTypeBuilder);

            Assert.Contains(principalEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "PostDetails");
            Assert.Contains(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "Post");
            Assert.DoesNotContain(dependentEntityTypeBuilder.Metadata.Navigations, nav => nav.Name == "AnotherPost");
        }

        [Fact]
        public void InversePropertyAttribute_adds_relationship_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            var entityTypeBuilder = modelBuilder.Entity<PostDetails>();

            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(Post)).Navigations, nav => nav.Name == "PostDetails");
            Assert.Equal("AnotherPost", modelBuilder.Model.GetEntityType(typeof(Post)).Navigations.First(nav => nav.Name == "PostDetails").ForeignKey.DependentToPrincipal.Name);

            Assert.Contains(modelBuilder.Model.GetEntityType(typeof(PostDetails)).Navigations, nav => nav.Name == "Post");
            Assert.Null(modelBuilder.Model.GetEntityType(typeof(PostDetails)).Navigations.First(nav => nav.Name == "Post").ForeignKey.PrincipalToDependent);
        }

        [Fact]
        public void InversePropertyAttribute_throws_if_self_navigation()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());

            Assert.Equal(Strings.SelfReferencingNavigationWithInverseProperty("AnotherEntity", typeof(SelfReferencingEntity).FullName, "AnotherEntity", typeof(SelfReferencingEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<SelfReferencingEntity>()).Message);
        }

        [Fact]
        public void InversePropertyAttribute_throws_if_navigation_does_not_exist()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());

            Assert.Equal(Strings.InvalidNavigationWithInverseProperty("Post", typeof(WrongNavigationName).FullName, "Navigation", typeof(Post).FullName),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<WrongNavigationName>()).Message);
        }

        [Fact]
        public void InversePropertyAttribute_throws_if_navigation_return_type_is_wrong()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());

            Assert.Equal(Strings.InvalidNavigationWithInverseProperty("Post", typeof(WrongNavigationType).FullName, "Blog", typeof(Post).FullName),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity<WrongNavigationType>()).Message);
        }

        [Fact]
        public void InversePropertyAttribute_does_not_create_relationship_if_inverse_properties_are_not_pointing_at_each_other()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<MismatchedInverseProperties>();

            Assert.Equal(0, modelBuilder.Model.GetEntityType(typeof(MismatchedInverseProperties)).Navigations.Count());
            Assert.DoesNotContain(modelBuilder.Model.GetEntityType(typeof(Post)).Navigations, nav => nav.Name == "Mismatch");
        }

        #endregion

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventionSet);

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private class Blog
        {
            public int Id { get; set; }

            [NotMapped]
            public BlogDetails BlogDetails { get; set; }

            public Forum Forum { get; set; }

            public int PostId { get; set; }

            public Post Post { get; set; }
        }

        private class BlogDetails
        {
            public static readonly PropertyInfo BlogIdProperty = typeof(BlogDetails).GetProperty("BlogId");

            public int Id { get; set; }

            public int? BlogId { get; set; }

            [Required]
            public Blog Blog { get; set; }
        }

        private class Forum
        {
            public int Id { get; set; }

            [Required]
            public ICollection<Blog> Blogs { get; set; }
        }

        private class Post
        {
            public int Id { get; set; }

            [Required]
            public Blog Blog { get; set; }

            [InverseProperty("AnotherPost")]
            public PostDetails PostDetails { get; set; }

            [InverseProperty("AnotherPost")]
            public MismatchedInverseProperties Mismatch { get; set; }
        }

        private class PostDetails
        {
            public static readonly PropertyInfo PostIdProperty = typeof(PostDetails).GetProperty("PostId");

            public int Id { get; set; }
            public int PostId { get; set; }

            public Post Post { get; set; }
            public Post AnotherPost { get; set; }
        }

        private class SelfReferencingEntity
        {
            public int Id { get; set; }

            [InverseProperty("AnotherEntity")]
            public SelfReferencingEntity AnotherEntity { get; set; }
        }

        private class WrongNavigationName
        {
            public int Id { get; set; }

            [InverseProperty("Navigation")]
            public Post Post { get; set; }
        }

        private class WrongNavigationType
        {
            public int Id { get; set; }

            [InverseProperty("Blog")]
            public Post Post { get; set; }
        }

        private class MismatchedInverseProperties
        {
            public int Id { get; set; }

            [InverseProperty("Mismatch")]
            public Post Post { get; set; }

            public Post AnotherPost { get; set; }

        }
    }
}