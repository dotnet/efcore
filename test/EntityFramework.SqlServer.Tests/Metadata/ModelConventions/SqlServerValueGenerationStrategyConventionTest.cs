// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata.ModelConventions;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata.ModelConventions
{
    class SqlServerValueGenerationStrategyConventionTest
    {
        public class SampleEntity
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
        }

        public class ReferencedEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
        }


        [Fact]
        public void Default_annotation_is_set_for_primary_key()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(1, property.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Default.ToString(), property[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration]);
        }

        [Fact]
        public void Default_annotation_is_not_set_for_non_primary_key()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.Key(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            Assert.Equal(0, keyBuilder.Metadata.Properties[0].Annotations.Count());
        }

        [Fact]
        public void No_annotation_set_when_composite_primary_key()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(0, keyProperties[0].Annotations.Count());
            Assert.Equal(0, keyProperties[1].Annotations.Count());
        }

        [Fact]
        public void No_annotation_set_when_primary_key_property_is_non_integer()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Name" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            Assert.Equal(0, keyBuilder.Metadata.Properties[0].Annotations.Count());
        }

        [Fact]
        public void No_annotation_set_when_primary_key_property_has_generate_value_false()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);
            keyBuilder.Metadata.Properties.First().GenerateValueOnAdd = false;

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            Assert.Equal(0, keyBuilder.Metadata.Properties[0].Annotations.Count());
        }

        [Fact]
        public void Convention_does_not_override_annotation_when_configured_explicitly()
        {
            var modelBuilder = createInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);
            keyBuilder.Metadata.Properties.First().SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            Assert.Equal(1, keyBuilder.Metadata.Properties[0].Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Identity.ToString(), keyBuilder.Metadata.Properties[0].Annotations.First().Value);
        }

        [Fact]
        public void Annotation_is_removed_when_foreign_key_is_added()
        {
            var modelBuilder = createInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(1, keyProperties[0].Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Default.ToString(), keyProperties[0].Annotations.First().Value);

            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Equal(0, keyProperties[0].Annotations.Count());
        }

        [Fact]
        public void Annotation_is_added_when_foreign_key_is_removed_and_key_is_primary_key()
        {
            var modelBuilder = createInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(1, keyProperties[0].Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Default.ToString(), keyProperties[0].Annotations.First().Value);

            var relationshipBuilder = principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Equal(0, keyProperties[0].Annotations.Count());

            referencedEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new SqlServerValueGenerationStrategyConvention().Apply(keyBuilder));

            Assert.Equal(1, keyProperties[0].Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Default.ToString(), keyProperties[0].Annotations.First().Value);

        }

        [Fact]
        public void Annotation_is_added_when_conventional_model_builder_is_used()
        {
            var model = new SqlServerModelBuilderFactory().CreateConventionBuilder(new Model()).Model;

            Assert.Equal(1, model.Annotations.Count());
            Assert.Equal(SqlServerValueGenerationStrategy.Sequence.ToString(), model.Annotations.First().Value);

        }

        private static InternalModelBuilder createInternalModelBuilder()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertiesConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());

            var keyConvention = new KeyConvention();

            conventions.KeyAddedConventions.Add(keyConvention);
            conventions.ForeignKeyRemovedConventions.Add(keyConvention);

            var sqlServerValueGenerationStrategyConvention = new SqlServerValueGenerationStrategyConvention();
            conventions.ForeignKeyAddedConventions.Add(sqlServerValueGenerationStrategyConvention);
            conventions.ForeignKeyRemovedConventions.Add(sqlServerValueGenerationStrategyConvention);

            return new InternalModelBuilder(new Model(), conventions);
        }
    }
}
