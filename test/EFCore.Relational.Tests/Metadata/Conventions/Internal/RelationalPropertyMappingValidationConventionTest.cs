// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class RelationalPropertyMappingValidationConventionTest : PropertyMappingValidationConventionTest
    {
        [ConditionalFact]
        public void Throws_when_added_property_is_not_mapped_to_store()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty", ConfigurationSource.Explicit);
            entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property), ConfigurationSource.Explicit);

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(), "LongProperty", typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public void Throws_when_added_property_is_not_mapped_to_store_even_if_configured_to_use_column_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveNonNavigationAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(Tuple<long>), "LongProperty", ConfigurationSource.Explicit)
                .HasColumnType("some_int_mapping");

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveNonNavigationAsPropertyEntity).ShortDisplayName(), "LongProperty",
                    typeof(Tuple<long>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        protected override TestHelpers TestHelpers => RelationalTestHelpers.Instance;

        protected override IModelValidator CreateModelValidator()
        {
            var typeMappingSource = new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            return new RelationalModelValidator(
                new ModelValidatorDependencies(
                    typeMappingSource,
                    new MemberClassifier(
                        typeMappingSource,
                        TestServiceFactory.Instance.Create<IParameterBindingFactories>())),
                new RelationalModelValidatorDependencies(typeMappingSource));
        }
    }
}
