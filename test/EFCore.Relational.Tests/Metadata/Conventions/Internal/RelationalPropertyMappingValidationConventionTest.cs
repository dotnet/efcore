// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalPropertyMappingValidationConventionTest : PropertyMappingValidationConventionTest
    {
        [Fact]
        public void Throws_when_added_property_is_not_mapped_to_store()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("LongProperty", typeof(Tuple<long>), ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property), ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(), "LongProperty", typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Throws_when_added_property_is_not_mapped_to_store_even_if_configured_to_use_column_type()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveNonNavigationAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("LongProperty", typeof(Tuple<long>), ConfigurationSource.Explicit).Relational(ConfigurationSource.Convention).HasColumnType("some_int_mapping");

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveNonNavigationAsPropertyEntity).ShortDisplayName(), "LongProperty", typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreateConvention().Apply(modelBuilder)).Message);
        }

        protected override PropertyMappingValidationConvention CreateConvention()
            => new PropertyMappingValidationConvention(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                TestServiceFactory.Instance.Create<IMemberClassifier>());
    }
}
