// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class PropertyMappingValidationConventionTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Throws_when_added_property_is_not_of_primitive_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property(
                typeof(NavigationAsProperty), nameof(NonPrimitiveAsPropertyEntity.Property), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotMapped(
                    typeof(NonPrimitiveAsPropertyEntity).ShortDisplayName(),
                    nameof(NonPrimitiveAsPropertyEntity.Property),
                    typeof(NavigationAsProperty).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_added_shadow_property_by_convention_is_not_of_primitive_type()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NonPrimitiveAsPropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(NavigationAsProperty), "ShadowProperty", ConfigurationSource.Convention);
            entityTypeBuilder.Ignore(nameof(NonPrimitiveAsPropertyEntity.Property), ConfigurationSource.Explicit);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Throws_when_primitive_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotAdded(
                    typeof(PrimitivePropertyEntity).ShortDisplayName(), "Property", typeof(int).DisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_when_nonprimitive_value_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(NonPrimitiveValueTypePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotAdded(
                    typeof(NonPrimitiveValueTypePropertyEntity).ShortDisplayName(), "Property", typeof(CancellationToken).Name),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Throws_when_keyless_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(NonPrimitiveReferenceTypePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.PropertyNotAdded(
                    typeof(NonPrimitiveReferenceTypePropertyEntity).ShortDisplayName(),
                    nameof(NonPrimitiveReferenceTypePropertyEntity.Property),
                    typeof(ICollection<Uri>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_primitive_type_property_is_added()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property(typeof(int), "Property", ConfigurationSource.Convention);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_primitive_type_property_is_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Property", ConfigurationSource.DataAnnotation);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Throws_when_navigation_is_not_added_or_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.NavigationNotAdded(
                    typeof(NavigationEntity).ShortDisplayName(), "Navigation", typeof(PrimitivePropertyEntity).Name),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_navigation_is_added()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasRelationship(referencedEntityTypeBuilder.Metadata, "Navigation", null, ConfigurationSource.Convention);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_navigation_is_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Ignore("Navigation", ConfigurationSource.DataAnnotation);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_navigation_target_entity_is_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(NavigationEntity), ConfigurationSource.Convention);
            modelBuilder.Ignore(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_explicit_navigation_is_not_added()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            var entityTypeBuilder = modelBuilder.Entity(typeof(ExplicitNavigationEntity), ConfigurationSource.Convention);
            var referencedEntityTypeBuilder = modelBuilder.Entity(typeof(PrimitivePropertyEntity), ConfigurationSource.Convention);
            referencedEntityTypeBuilder.Ignore("Property", ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasRelationship(
                referencedEntityTypeBuilder.Metadata, "Navigation", null, ConfigurationSource.Convention);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        [ConditionalFact]
        public virtual void Throws_when_interface_type_property_is_not_added_or_ignored()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(InterfaceNavigationEntity), ConfigurationSource.Convention);

            Assert.Equal(
                CoreStrings.InterfacePropertyNotAdded(
                    typeof(InterfaceNavigationEntity).ShortDisplayName(),
                    "Navigation",
                    typeof(IList<INavigationEntity>).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(() => CreatePropertyMappingValidator()(modelBuilder.Metadata)).Message);
        }

        [ConditionalFact]
        public virtual void Does_not_throw_when_non_candidate_property_is_not_added()
        {
            var modelBuilder = CreateConventionlessModelBuilder().GetInfrastructure();
            modelBuilder.Entity(typeof(NonCandidatePropertyEntity), ConfigurationSource.Convention);

            CreatePropertyMappingValidator()(modelBuilder.Metadata);
        }

        protected override ModelBuilder CreateConventionlessModelBuilder(bool sensitiveDataLoggingEnabled = false)
        {
            var conventionSet = new ConventionSet();

            var dependencies = CreateDependencies(sensitiveDataLoggingEnabled);
            conventionSet.ModelFinalizedConventions.Add(new TypeMappingConvention(dependencies));

            return new ModelBuilder(conventionSet);
        }

        protected virtual Action<IModel> CreatePropertyMappingValidator()
        {
            var validator = CreateModelValidator();
            var logger = new TestLogger<DbLoggerCategory.Model.Validation, TestLoggingDefinitions>();

            var validatePropertyMappingMethod = typeof(ModelValidator).GetRuntimeMethods().Single(e => e.Name == "ValidatePropertyMapping");

            return m =>
            {
                try
                {
                    ((Model)m).FinalizeModel();
                    validatePropertyMappingMethod.Invoke(
                        validator, new object[] { m, logger });
                }
                catch (TargetInvocationException exception)
                {
                    throw exception.InnerException;
                }
            };
        }

        protected virtual IModelValidator CreateModelValidator()
        {
            var typeMappingSource = new InMemoryTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>());

            return new ModelValidator(
                new ModelValidatorDependencies(
                    typeMappingSource,
                    new MemberClassifier(
                        typeMappingSource,
                        TestServiceFactory.Instance.Create<IParameterBindingFactories>())));
        }

        protected class NonPrimitiveNonNavigationAsPropertyEntity
        {
        }

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

        protected class NonPrimitiveReferenceTypePropertyEntity
        {
            public ICollection<Uri> Property { get; set; }
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
                set => _writeOnlyField = value;
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
