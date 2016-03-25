// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions.Internal
{
    public class PropertyMappingValidationConventionTest
    {
        [Fact]
        public virtual void Throws_when_added_property_is_not_of_primitive_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(NavigationAsProperty), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.PropertyNotMapped(
                typeof(NonPrimitiveAsPropertyEntity).DisplayName(fullName: false),
                "Property",
                typeof(NavigationAsProperty).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public virtual void Throws_when_primitive_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(PrimitivePropertyEntity).DisplayName(fullName: false), "Property", typeof(int).DisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public virtual void Throws_when_nonprimitive_value_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveValueTypePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotAdded(
                    typeof(NonPrimitiveValueTypePropertyEntity).DisplayName(fullName: false), "Property", typeof(CancellationToken).Name),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public virtual void Does_not_throw_when_primitive_type_property_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Property", typeof(int), ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Does_not_throw_when_primitive_type_property_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Throws_when_navigation_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.NavigationNotAdded(typeof(NavigationEntity).DisplayName(fullName: false), "Navigation", typeof(PrimitivePropertyEntity).Name),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public virtual void Does_not_throw_when_navigation_is_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);
            entityTypeBuilder.Relationship(referencedEntityTypeBuilder, "Navigation", null, ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Does_not_throw_when_navigation_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Navigation", ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Does_not_throw_when_navigation_target_entity_is_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            modelBuilder.Ignore(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Does_not_throw_when_explicit_navigation_is_not_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(ExplicitNavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.Convention);
            entityTypeBuilder.Relationship(referencedEntityTypeBuilder, "Navigation", null, ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Throws_when_interface_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(InterfaceNavigationEntity), ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.InterfacePropertyNotAdded(
                typeof(InterfaceNavigationEntity).DisplayName(fullName: false),
                "Navigation",
                typeof(IList<INavigationEntity>).DisplayName(fullName: false)),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public virtual void Does_not_throw_when_non_candidate_property_is_not_added()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonCandidatePropertyEntity), ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        [Fact]
        public virtual void Does_not_throw_when_clr_type_is_not_set_for_shadow_property()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationAsProperty), ConfigurationSource.Convention);
            entityTypeBuilder.Property("ShadowPropertyOfNullType", ConfigurationSource.Convention);

            CreateConvention().Apply(modelBuilder);
        }

        protected virtual PropertyMappingValidationConvention CreateConvention() => new PropertyMappingValidationConvention();

        protected class NonPrimitiveAsPropertyEntity
        {
            public NavigationAsProperty Property { get; set; }
        }

        protected class NavigationAsProperty
        {
        }

        protected class PrimitivePropertyEntity
        {
            public int Property { get; set; }
        }

        protected class NonPrimitiveValueTypePropertyEntity
        {
            public CancellationToken Property { get; set; }
        }

        protected class NavigationEntity
        {
            public PrimitivePropertyEntity Navigation { get; set; }
        }

        protected class NonCandidatePropertyEntity
        {
            public static int StaticProperty { get; set; }

            public int _writeOnlyField = 1;

            public int WriteOnlyProperty
            {
                set { _writeOnlyField = value; }
            }
        }

        protected interface INavigationEntity
        {
            PrimitivePropertyEntity Navigation { get; set; }
        }

        protected class ExplicitNavigationEntity : INavigationEntity
        {
            PrimitivePropertyEntity INavigationEntity.Navigation { get; set; }

            public PrimitivePropertyEntity Navigation { get; set; }
        }

        protected class InterfaceNavigationEntity
        {
            public IList<INavigationEntity> Navigation { get; set; }
        }
    }
}
