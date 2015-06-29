// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.Conventions
{
    public class KeyConventionTest
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

        #region IsValueGeneratedOnAdd

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_set_for_key_properties()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "Name" };
            var keyBuilder = entityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.NotNull(keyProperties[0].IsValueGeneratedOnAdd);
            Assert.NotNull(keyProperties[1].IsValueGeneratedOnAdd);

            Assert.True(keyProperties[0].IsValueGeneratedOnAdd.Value);
            Assert.True(keyProperties[1].IsValueGeneratedOnAdd.Value);
        }

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_not_set_for_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };
            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Null(keyProperties[0].IsValueGeneratedOnAdd);
        }

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_set_for_property_which_are_not_part_of_any_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };
            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.Key(new List<string> { "Id", "SampleEntityId" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].IsValueGeneratedOnAdd);
            Assert.Null(keyProperties[1].IsValueGeneratedOnAdd);
        }

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_not_set_for_properties_which_are_part_of_a_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "SampleEntityId" };
            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.Key(new List<string> { "SampleEntityId" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Null(keyProperties[0].IsValueGeneratedOnAdd);
        }

        [Fact]
        public void KeyConvention_does_not_override_IsValueGeneratedOnAdd_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).GenerateValueOnAdd(false, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].IsValueGeneratedOnAdd.Value);
        }

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_turned_off_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };
            var keyBuilder = referencedEntityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].IsValueGeneratedOnAdd);

            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Null(keyProperties[0].IsValueGeneratedOnAdd);
        }

        [Fact]
        public void IsValueGeneratedOnAdd_flag_is_set_when_foreign_key_is_removed()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };
            var keyBuilder = referencedEntityBuilder.Key(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].IsValueGeneratedOnAdd);

            var relationshipBuilder = principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Null(keyProperties[0].IsValueGeneratedOnAdd);

            referencedEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.True(keyProperties[0].IsValueGeneratedOnAdd);
        }

        #endregion

        #region Identity

        [Fact]
        public void Identity_is_set_for_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_is_not_set_for_non_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.Key(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Null(property.StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_not_set_when_composite_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Null(keyProperties[0].StoreGeneratedPattern);
            Assert.Null(keyProperties[1].StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_not_set_when_primary_key_property_is_non_integer()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Name" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Null(property.StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_is_recomputed_when_primary_key_is_changed()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var idProperty = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;
            var numberProperty = entityBuilder.Property(typeof(int), "Number", ConfigurationSource.Convention).Metadata;

            Assert.Equal(StoreGeneratedPattern.Identity, idProperty.StoreGeneratedPattern);
            Assert.Null(numberProperty.StoreGeneratedPattern);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            Assert.Null(idProperty.StoreGeneratedPattern);
            Assert.Equal(StoreGeneratedPattern.Identity, numberProperty.StoreGeneratedPattern);
        }

        [Fact]
        public void Convention_does_not_override_None_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention)
                .StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(StoreGeneratedPattern.None, property.StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_is_removed_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);

            principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Null(property.StoreGeneratedPattern);
        }

        [Fact]
        public void Identity_is_added_when_foreign_key_is_removed_and_key_is_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);

            var relationshipBuilder = principalEntityBuilder.Relationship(
                principalEntityBuilder,
                referencedEntityBuilder,
                null,
                null,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                null,
                ConfigurationSource.Convention);

            Assert.Null(property.StoreGeneratedPattern);

            referencedEntityBuilder.RemoveRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            Assert.Equal(StoreGeneratedPattern.Identity, property.StoreGeneratedPattern);

        }

        #endregion

        private static InternalModelBuilder CreateInternalModelBuilder()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention());
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention());

            var keyConvention = new KeyConvention();

            conventions.KeyAddedConventions.Add(keyConvention);
            conventions.ForeignKeyRemovedConventions.Add(keyConvention);

            return new InternalModelBuilder(new Model(), conventions);
        }
    }
}
