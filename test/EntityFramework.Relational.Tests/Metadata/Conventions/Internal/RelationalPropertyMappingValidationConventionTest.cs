// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Metadata.Conventions.Internal
{
    public class RelationalPropertyMappingValidationConventionTest
    {
        [Fact]
        public void Throws_when_added_property_is_not_mapped_to_store()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(long), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotMapped("Property", typeof(NonPrimitiveAsPropertyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_when_added_property_is_configured_to_use_column_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(long), ConfigurationSource.Convention).Relational(ConfigurationSource.Convention).ColumnType("some_int_mapping");

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Throws_when_primitive_type_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotAdded("Property", typeof(PrimitivePropertyEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_when_primitive_type_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(int), ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_primitive_type_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Throws_when_navigation_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.NavigationNotAdded("Navigation", typeof(NavigationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_when_navigation_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);
            entityTypeBuilder.Relationship(referencedEntityTypeBuilder, "Navigation", null, ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_navigation_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Navigation", ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_navigation_target_entity_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            modelBuilder.Ignore(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
        }

        [Fact]
        public void Does_not_throw_when_interface_or_non_candidate_property_is_not_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityTypeBuilder = modelBuilder.Entity(typeof(InterfacePropertyEntity), ConfigurationSource.Convention);

            new RelationalPropertyMappingValidationConvention(new TestRelationalTypeMapper()).Apply(modelBuilder);
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

        private class InterfacePropertyEntity
        {
            public IProperty InterfaceProperty { get; set; }

            public int ReadOnlyProperty { get; }

            public static int StaticProperty { get; set; }
        }
    }
}
