// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Conventions
{
    public class NavigationAttributeConventionTest
    {
        #region RequiredAttribute

        [Fact]
        public void RequiredAttribute_overrides_configuration_from_convention_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();

            var relationshipBuilder = entityTypeBuilder.ForeignKey(typeof(Blog), entityTypeBuilder.Metadata.ClrType.GetProperties().Where(p => p.Name == "BlogId").ToList(), ConfigurationSource.Convention);
            var navigation = entityTypeBuilder.Metadata.FindNavigation("Blog");

            relationshipBuilder.Required(false, ConfigurationSource.Convention);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = new RequiredNavigationAttributeConvention().Apply(relationshipBuilder, navigation);

            Assert.True(relationshipBuilder.Metadata.IsRequired);
        }

        [Fact]
        public void RequiredAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<BlogDetails>();

            var relationshipBuilder = entityTypeBuilder.ForeignKey(typeof(Blog), entityTypeBuilder.Metadata.ClrType.GetProperties().Where(p => p.Name == "BlogId").ToList(), ConfigurationSource.Convention);
            var navigation = entityTypeBuilder.Metadata.FindNavigation("Blog");

            relationshipBuilder.Required(false, ConfigurationSource.Explicit);

            Assert.False(relationshipBuilder.Metadata.IsRequired);

            relationshipBuilder = new RequiredNavigationAttributeConvention().Apply(relationshipBuilder, navigation);

            Assert.False(relationshipBuilder.Metadata.IsRequired);
        }

        //[Fact]
        //public void RequiredAttribute_sets_is_required_with_conventional_builder()
        //{
        //    var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
        //    var entityTypeBuilder = modelBuilder.Entity<BlogDetails>();

        //    Assert.True(entityTypeBuilder.Metadata.GetForeignKeys().ToList()[0].IsRequired);
        //}

        #endregion

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());
            conventionSet.EntityTypeAddedConventions.Add(new RelationshipDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventionSet);

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        public class Blog
        {
            public int Id { get; set; }

            public virtual BlogDetails BlogDetails { get; set; }
        }

        public class BlogDetails
        {
            public int Id { get; set; }

            public int? BlogId { get; set; }

            [Required]
            public Blog Blog { get; set; }
        }
    }
}
