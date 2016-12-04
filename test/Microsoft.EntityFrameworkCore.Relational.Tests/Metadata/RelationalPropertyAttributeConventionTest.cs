// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Metadata
{
    public class RelationalPropertyAttributeConventionTest
    {
        [Fact]
        public void ColumnAttribute_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("Post Name", entityBuilder.Property(e => e.Name).Metadata.Relational().ColumnName);
            Assert.Equal("DECIMAL", entityBuilder.Property(e => e.Name).Metadata.Relational().ColumnType);
        }

        [Fact]
        public void ColumnAttribute_on_field_sets_column_name_and_type_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());

            var entityBuilder = modelBuilder.Entity<F>();

            Assert.Equal("Post Name", entityBuilder.Property<string>(nameof(F.Name)).Metadata.Relational().ColumnName);
            Assert.Equal("DECIMAL", entityBuilder.Property<string>(nameof(F.Name)).Metadata.Relational().ColumnType);
        }

        [Fact]
        public void ColumnAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property("Name", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "ConventionalName", ConfigurationSource.Convention);
            propertyBuilder.HasAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "BYTE", ConfigurationSource.Convention);

            new RelationalColumnAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("Post Name", propertyBuilder.Metadata.Relational().ColumnName);
            Assert.Equal("DECIMAL", propertyBuilder.Metadata.Relational().ColumnType);
        }

        [Fact]
        public void ColumnAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property("Name", typeof(string), ConfigurationSource.Explicit);

            propertyBuilder.HasAnnotation(RelationalFullAnnotationNames.Instance.ColumnName, "ExplicitName", ConfigurationSource.Explicit);
            propertyBuilder.HasAnnotation(RelationalFullAnnotationNames.Instance.ColumnType, "BYTE", ConfigurationSource.Explicit);

            new RelationalColumnAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("ExplicitName", propertyBuilder.Metadata.Relational().ColumnName);
            Assert.Equal("BYTE", propertyBuilder.Metadata.Relational().ColumnType);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

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
