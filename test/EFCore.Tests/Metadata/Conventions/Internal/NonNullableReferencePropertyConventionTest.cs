// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class NonNullableReferencePropertyConventionTest
    {
        [Fact]
        public void Non_nullability_does_not_override_configuration_from_explicit_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.Explicit);

            new NonNullableReferencePropertyConvention(new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions>()).Apply(
                propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [Fact]
        public void Non_nullability_does_not_override_configuration_from_data_annotation_source()
        {
            var entityTypeBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityTypeBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.IsRequired(false, ConfigurationSource.DataAnnotation);

            new NonNullableReferencePropertyConvention(new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions>()).Apply(
                propertyBuilder);

            Assert.True(propertyBuilder.Metadata.IsNullable);
        }

        [Fact]
        public void Non_nullability_sets_is_nullable_with_conventional_builder()
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.False(entityTypeBuilder.Property(e => e.Name).Metadata.IsNullable);
        }

        [Theory]
        [InlineData(nameof(A.NullAwareNonNullable), false)]
        [InlineData(nameof(A.NullAwareNullable), true)]
        [InlineData(nameof(A.NullObliviousNonNullable), true)]
        [InlineData(nameof(A.NullObliviousNullable), true)]
        [InlineData(nameof(A.RequiredAndNullable), false)]
        public void Reference_nullability_sets_is_nullable_correctly(string propertyName, bool expectedNullable)
        {
            var modelBuilder = CreateModelBuilder();
            var entityTypeBuilder = modelBuilder.Entity<A>();

            Assert.Equal(expectedNullable, entityTypeBuilder.Property(propertyName).Metadata.IsNullable);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(
                    CreateTypeMapper(),
                    new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions>()));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private static ITypeMappingSource CreateTypeMapper()
            => TestServiceFactory.Instance.Create<InMemoryTypeMappingSource>();

        private class A
        {
            public int Id { get; set; }

#nullable enable
            public string Name { get; set; } = "";

            public string NullAwareNonNullable { get; set; } = "";
            public string? NullAwareNullable { get; set; }

            [Required]
            public string? RequiredAndNullable { get; set; }
#nullable disable

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' context.
            public string NullObliviousNonNullable { get; set; }
            public string? NullObliviousNullable { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' context.
        }

        private static ModelBuilder CreateModelBuilder() => InMemoryTestHelpers.Instance.CreateConventionBuilder();
    }
}
