// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalPropertyAttributeConventionTest
    {
        public class A
        {
            public int Id { get; set; }

            [Column("Post Name", Order = 1, TypeName = "DECIMAL")]
            public string Name { get; set; }
        }

        [Fact]
        public void ColumnAttribute_sets_column_name_order_and_type_with_conventional_builder()
        {
            var modelBuilder = new ModelBuilder(new TestConventionalSetBuilder().AddConventions(new CoreConventionSetBuilder().CreateConventionSet()));

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("Post Name", entityBuilder.Property(e => e.Name).Metadata.Relational().Column);
            Assert.Equal(1, entityBuilder.Property(e => e.Name).Metadata.Relational().ColumnOrder);
            Assert.Equal("DECIMAL", entityBuilder.Property(e => e.Name).Metadata.Relational().ColumnType);
        }


        [Fact]
        public void ColumnAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName, "ConventionalName", ConfigurationSource.Convention);
            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnOrder, 3, ConfigurationSource.Convention);
            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Convention);

            new RelationalColumnAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("Post Name", propertyBuilder.Metadata.Relational().Column);
            Assert.Equal(1, propertyBuilder.Metadata.Relational().ColumnOrder);
            Assert.Equal("DECIMAL", propertyBuilder.Metadata.Relational().ColumnType);

        }

        [Fact]
        public void ColumnAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            var propertyBuilder = entityBuilder.Property(typeof(string), "Name", ConfigurationSource.Explicit);

            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName, "ExplicitName", ConfigurationSource.Explicit);
            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnOrder, 0, ConfigurationSource.Explicit);
            propertyBuilder.Annotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType, "BYTE", ConfigurationSource.Explicit);

            new RelationalColumnAttributeConvention().Apply(propertyBuilder);

            Assert.Equal("ExplicitName", propertyBuilder.Metadata.Relational().Column);
            Assert.Equal(0, propertyBuilder.Metadata.Relational().ColumnOrder);
            Assert.Equal("BYTE", propertyBuilder.Metadata.Relational().ColumnType);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(), conventionSet);

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private class TestConventionalSetBuilder : RelationalConventionSetBuilder
        {
        }
    }
}
