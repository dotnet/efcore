// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class ValueGeneratorConventionTest
    {
        private class SampleEntity
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string Name { get; set; }
        }

        private class ReferencedEntity
        {
            public int Id { get; set; }
            public int SampleEntityId { get; set; }
        }

        private enum Eenom
        {
            E,
            Nom
        }

        #region RequiresValueGenerator

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_key_properties_that_use_value_generation()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "Name" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator());
            Assert.False(keyProperties[1].RequiresValueGenerator());

            Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
            Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_not_set_for_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator());
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_property_which_are_not_part_of_any_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "SampleEntityId" };
            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);
            referencedEntityBuilder.Property(properties[1], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(new[] { properties[1] }, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator());
            Assert.False(keyProperties[1].RequiresValueGenerator());
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_not_set_for_properties_which_are_part_of_a_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(new[] { properties[1] }, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator());
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void KeyConvention_does_not_override_ValueGenerated_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_turned_off_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator());

            var foreignKeyBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(foreignKeyBuilder, new ValueGeneratorConvention().Apply(foreignKeyBuilder));

            Assert.False(keyProperties[0].RequiresValueGenerator());
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_when_foreign_key_is_removed()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator());

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new ValueGeneratorConvention().Apply(relationshipBuilder));

            Assert.False(keyProperties[0].RequiresValueGenerator());
            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            new ValueGeneratorConvention().Apply(referencedEntityBuilder, relationshipBuilder.Metadata);

            Assert.True(keyProperties[0].RequiresValueGenerator());
            Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
        }

        #endregion

        #region Identity

        [Fact]
        public void Identity_is_set_for_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Identity_is_not_set_for_non_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.HasKey(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Identity_not_set_when_composite_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
            Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
        }

        [Fact]
        public void Identity_set_when_primary_key_property_is_string()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Name" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator());
        }

        [Fact]
        public void Identity_set_when_primary_key_property_is_byte_array()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityBuilder.Property("binaryKey", typeof(byte[]), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { "binaryKey" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.True(property.RequiresValueGenerator());
        }

        [Fact]
        public void Identity_not_set_when_primary_key_property_is_enum()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityBuilder.Property("enumKey", typeof(Eenom), ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new[] { "enumKey" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
            Assert.False(property.RequiresValueGenerator());
        }

        [Fact]
        public void Identity_is_recomputed_when_primary_key_is_changed()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var idProperty = entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).Metadata;
            var numberProperty = entityBuilder.Property("Number", typeof(int), ConfigurationSource.Convention).Metadata;

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, numberProperty.ValueGenerated);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Number" }, ConfigurationSource.Convention);
            Assert.NotNull(keyBuilder);

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.Equal(ValueGenerated.Never, ((IProperty)idProperty).ValueGenerated);
            Assert.Equal(ValueGenerated.OnAdd, ((IProperty)numberProperty).ValueGenerated);
        }

        [Fact]
        public void Convention_does_not_override_None_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(entityBuilder, (Key)null));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Identity_is_removed_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, ((IProperty)property).ValueGenerated);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new ValueGeneratorConvention().Apply(relationshipBuilder));

            Assert.Equal(ValueGenerated.Never, ((IProperty)property).ValueGenerated);
        }

        [Fact]
        public void Identity_is_added_when_foreign_key_is_removed_and_key_is_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };
            var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new ValueGeneratorConvention().Apply(relationshipBuilder));

            Assert.Equal(ValueGenerated.Never, ((IProperty)property).ValueGenerated);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.True(new ValueGeneratorConvention().Apply(referencedEntityBuilder, (Key)null));

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        #endregion

        private static InternalModelBuilder CreateInternalModelBuilder()
        {
            var conventions = new ConventionSet();

            conventions.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention(
                TestServiceFactory.Instance.Create<CoreTypeMapper>()));
            conventions.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention(new TestLogger<DbLoggerCategory.Model>()));

            var keyConvention = new ValueGeneratorConvention();

            conventions.ForeignKeyAddedConventions.Add(keyConvention);
            conventions.ForeignKeyRemovedConventions.Add(keyConvention);
            conventions.PrimaryKeyChangedConventions.Add(keyConvention);

            return new InternalModelBuilder(new Model(conventions));
        }
    }
}
