// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class NonNullableReferencePropertyConventionTest
    {
        [ConditionalFact]
        public void Non_nullability_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.Explicit);

            RunConvention(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [ConditionalFact]
        public void Non_nullability_does_not_override_configuration_from_data_annotation_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

            RunConvention(propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [ConditionalFact]
        public void Non_nullability_sets_is_nullable_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Property(e => e.NonNullable).Metadata.IsNullable);
        }

        [ConditionalTheory]
        [InlineData(typeof(A), nameof(A.NonNullable), false)]
        [InlineData(typeof(A), nameof(A.Nullable), true)]

        [InlineData(typeof(A), nameof(A.NonNullablePropertyMaybeNull), true)]
        [InlineData(typeof(A), nameof(A.NonNullablePropertyAllowNull), false)]
        [InlineData(typeof(A), nameof(A.NullablePropertyNotNull), true)]
        [InlineData(typeof(A), nameof(A.NullablePropertyDisallowNull), true)]

        [InlineData(typeof(A), nameof(A.NonNullableFieldMaybeNull), true)]
        [InlineData(typeof(A), nameof(A.NonNullableFieldAllowNull), false)]
        [InlineData(typeof(A), nameof(A.NullableFieldNotNull), true)]
        [InlineData(typeof(A), nameof(A.NullableFieldDisallowNull), true)]

        [InlineData(typeof(A), nameof(A.RequiredAndNullable), false)]
        [InlineData(typeof(A), nameof(A.NullObliviousNonNullable), true)]
        [InlineData(typeof(A), nameof(A.NullObliviousNullable), true)]

        [InlineData(typeof(B), nameof(B.NonNullableValueType), false)]
        [InlineData(typeof(B), nameof(B.NullableValueType), true)]
        [InlineData(typeof(B), nameof(B.NonNullableRefType), false)]
        [InlineData(typeof(B), nameof(B.NullableRefType), true)]
        public void Reference_nullability_sets_is_nullable_correctly(Type type, string propertyName, bool expectedNullable)
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity(type);

            Assert.Equal(expectedNullable, entityTypeBuilder.Property(propertyName).Metadata.IsNullable);
        }

        private void RunConvention(InternalPropertyBuilder propertyBuilder)
        {
            var context = new ConventionContext<IConventionPropertyBuilder>(
                propertyBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher);

            new NonNullableReferencePropertyConvention(CreateDependencies())
                .ProcessPropertyAdded(propertyBuilder, context);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(CreateDependencies()));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private class A
        {
            public int Id { get; set; }

#nullable enable
            public string NonNullable { get; set; } = "";
            public string? Nullable { get; set; }

            [MaybeNull]
            public string NonNullablePropertyMaybeNull { get; set; } = "";
            [AllowNull]
            public string NonNullablePropertyAllowNull { get; set; } = "";
            [NotNull]
            public string? NullablePropertyNotNull { get; set; } = "";
            [DisallowNull]
            public string? NullablePropertyDisallowNull { get; set; } = "";

            [MaybeNull]
            public string NonNullableFieldMaybeNull = "";
            [AllowNull]
            public string NonNullableFieldAllowNull = "";
            [NotNull]
            public string? NullableFieldNotNull = "";
            [DisallowNull]
            public string? NullableFieldDisallowNull = "";

            [Required]
            public string? RequiredAndNullable { get; set; }
#nullable disable

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' context.
            public string NullObliviousNonNullable { get; set; }
            public string? NullObliviousNullable { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' context.
        }

#nullable enable
        public class B
        {
            [Key] public Guid NonNullableValueType { get; set; }
            public Guid? NullableValueType { get; set; }
            public string NonNullableRefType { get; set; } = "";
            public string? NullableRefType { get; set; }
        }
#nullable disable

        private static ModelBuilder CreateModelBuilder() => InMemoryTestHelpers.Instance.CreateConventionBuilder();
    }
}
