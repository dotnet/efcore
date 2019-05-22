// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalPropertyAttributeConventionTest
    {
        [Fact]
        public void ColumnAttribute_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("Post Name", entityBuilder.Property(e => e.Name).Metadata.GetColumnName());
            Assert.Equal("DECIMAL", entityBuilder.Property(e => e.Name).Metadata.GetColumnType());
        }

        [Fact]
        public void ColumnAttribute_on_field_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var entityBuilder = modelBuilder.Entity<F>();

            Assert.Equal("Post Name", entityBuilder.Property<string>(nameof(F.Name)).Metadata.GetColumnName());
            Assert.Equal("DECIMAL", entityBuilder.Property<string>(nameof(F.Name)).Metadata.GetColumnType());
        }

        [Fact]
        public void ColumnAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, "ConventionalName", ConfigurationSource.Convention);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Convention);

            new RelationalColumnAttributeConvention(new TestLogger<DbLoggerCategory.Model, TestRelationalLoggingDefinitions>()).Apply(propertyBuilder);

            Assert.Equal("Post Name", propertyBuilder.Metadata.GetColumnName());
            Assert.Equal("DECIMAL", propertyBuilder.Metadata.GetColumnType());
        }

        [Fact]
        public void ColumnAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnName, "ExplicitName", ConfigurationSource.Explicit);
            propertyBuilder.HasAnnotation(RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Explicit);

            new RelationalColumnAttributeConvention(new TestLogger<DbLoggerCategory.Model, TestRelationalLoggingDefinitions>()).Apply(propertyBuilder);

            Assert.Equal("ExplicitName", propertyBuilder.Metadata.GetColumnName());
            Assert.Equal("BYTE", propertyBuilder.Metadata.GetColumnType());
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(
                new PropertyDiscoveryConvention(
                    new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                    new TestLogger<DbLoggerCategory.Model, TestRelationalLoggingDefinitions>()));

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        protected virtual ModelBuilder CreateConventionalModelBuilder()
            => RelationalTestHelpers.Instance.CreateConventionBuilder();

        private class A
        {
            public int Id { get; set; }

            [Column("Post Name", Order = 1, TypeName = "DECIMAL")]
            public string Name { get; set; }
        }

        public class F
        {
            public int Id { get; set; }

            [Column("Post Name", Order = 1, TypeName = "DECIMAL")]
            public string Name;
        }
    }
}
