// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Metadata
{
    public class RelationalEntityTypeAttributeConventionTest
    {
        [Fact]
        public void TableAttribute_sets_column_name_order_and_type_with_conventional_builder()
        {
            var typeMapperMock = new Mock<IRelationalTypeMapper>();
            var modelBuilder = new ModelBuilder(new TestConventionalSetBuilder(typeMapperMock.Object).AddConventions(new CoreConventionSetBuilder().CreateConventionSet()));

            var entityBuilder = modelBuilder.Entity<A>();

            Assert.Equal("MyTable", entityBuilder.Metadata.Relational().TableName);
            Assert.Equal("MySchema", entityBuilder.Metadata.Relational().Schema);
        }

        [Fact]
        public void TableAttribute_overrides_configuration_from_convention_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            entityBuilder.HasAnnotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName, "ConventionalName", ConfigurationSource.Convention);
            entityBuilder.HasAnnotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema, "ConventionalSchema", ConfigurationSource.Convention);

            new RelationalTableAttributeConvention().Apply(entityBuilder);

            Assert.Equal("MyTable", entityBuilder.Metadata.Relational().TableName);
            Assert.Equal("MySchema", entityBuilder.Metadata.Relational().Schema);
        }

        [Fact]
        public void TableAttribute_does_not_override_configuration_from_explicit_source()
        {
            var entityBuilder = CreateInternalEntityTypeBuilder<A>();

            entityBuilder.HasAnnotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.TableName, "ExplicitName", ConfigurationSource.Explicit);
            entityBuilder.HasAnnotation(RelationalAnnotationNames.Prefix + RelationalAnnotationNames.Schema, "ExplicitName", ConfigurationSource.Explicit);

            new RelationalTableAttributeConvention().Apply(entityBuilder);

            Assert.Equal("ExplicitName", entityBuilder.Metadata.Relational().TableName);
            Assert.Equal("ExplicitName", entityBuilder.Metadata.Relational().Schema);
        }

        private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
        {
            var conventionSet = new ConventionSet();
            conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());

            var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

            return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
        }

        private class TestConventionalSetBuilder : RelationalConventionSetBuilder
        {
            public TestConventionalSetBuilder([NotNull] IRelationalTypeMapper typeMapper)
                : base(typeMapper)
            {
            }
        }

        [Table("MyTable", Schema = "MySchema")]
        private class A
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
