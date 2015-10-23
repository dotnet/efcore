// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Conventions
{
    public class PropertyMappingValidationConventionTest
    {
        [Fact]
        public void Throws_when_added_property_is_not_primitive_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(NavigationAsProperty), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotMapped("Property", typeof(NonPrimitiveAsPropertyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new PropertyMappingValidationConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Throws_when_primitive_type_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotAdded("Property", typeof(PrimitivePropertyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new PropertyMappingValidationConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_when_primitive_type_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(int), ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_primitive_type_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Throws_when_navigation_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.NavigationNotAdded("Navigation", typeof(NavigationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new PropertyMappingValidationConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_when_navigation_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);
            entityTypeBuilder.Relationship(referencedEntityTypeBuilder, "Navigation", null, ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_navigation_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Navigation", ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_navigation_target_entity_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            modelBuilder.Ignore(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_non_candidate_property_is_not_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonCandidatePropertyEntity), ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_clr_type_is_not_set_for_shadow_property()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationAsProperty), ConfigurationSource.Convention);
            entityTypeBuilder.Property("ShadowPropertyOfNullType", ConfigurationSource.Convention);

            new PropertyMappingValidationConvention().Apply(modelBuilder);
        }

        private class NonPrimitiveAsPropertyEntity
        {
            public NavigationAsProperty Property { get; set; }
        }

        private class NavigationAsProperty
        {
        }

        private class PrimitivePropertyEntity
        {
            public int Property { get; set; }
        }

        private class NavigationEntity
        {
            public PrimitivePropertyEntity Navigation { get; set; }
        }

        private class NonCandidatePropertyEntity
        {
            public static int StaticProperty { get; set; }

            public int _writeOnlyField = 1;
            public int WriteOnlyProperty
            {
                set { _writeOnlyField = value; }
            }
        }
    }
}
