// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Conventions
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

        #region RequiresValueGenerator

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_key_properties_that_use_value_generation()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id", "Name" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.HasKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);
            Assert.False(keyProperties[1].RequiresValueGenerator);

            Assert.True(((IProperty)keyProperties[0]).RequiresValueGenerator);
            Assert.False(((IProperty)keyProperties[1]).RequiresValueGenerator);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_not_set_for_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.HasKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_for_property_which_are_not_part_of_any_foreign_key()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            var keyBuilder = referencedEntityBuilder.HasKey(new List<string> { "Id", "SampleEntityId" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);
            Assert.False(keyProperties[1].RequiresValueGenerator);
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

            var keyBuilder = referencedEntityBuilder.HasKey(new List<string> { "SampleEntityId" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
        }

        [Fact]
        public void KeyConvention_does_not_override_RequiresValueGenerator_when_configured_explicitly()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "Id" };

            entityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            entityBuilder.Property("Id", typeof(int), ConfigurationSource.Convention).RequiresValueGenerator(false, ConfigurationSource.Explicit);

            var keyBuilder = entityBuilder.HasKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.False(keyProperties[0].RequiresValueGenerator);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_turned_off_when_foreign_key_is_added()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = referencedEntityBuilder.HasKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(((IProperty)keyProperties[0]).RequiresValueGenerator);

            var foreignKeyBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(foreignKeyBuilder, new KeyConvention().Apply(foreignKeyBuilder));

            Assert.False(((IProperty)keyProperties[0]).RequiresValueGenerator);
        }

        [Fact]
        public void RequiresValueGenerator_flag_is_set_when_foreign_key_is_removed()
        {
            var modelBuilder = CreateInternalModelBuilder();

            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            var properties = new List<string> { "SampleEntityId" };

            referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
                .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            var keyBuilder = referencedEntityBuilder.HasKey(properties, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.True(keyProperties[0].RequiresValueGenerator);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new KeyConvention().Apply(relationshipBuilder));

            Assert.False(((IProperty)keyProperties[0]).RequiresValueGenerator);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            new KeyConvention().Apply(referencedEntityBuilder, relationshipBuilder.Metadata);

            Assert.True(((IProperty)keyProperties[0]).RequiresValueGenerator);
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

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Identity_is_not_set_for_non_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.HasKey(new List<string> { "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Identity_not_set_when_composite_primary_key()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var keyProperties = keyBuilder.Metadata.Properties;

            Assert.Equal(ValueGenerated.Never, ((IProperty)keyProperties[0]).ValueGenerated);
            Assert.Equal(ValueGenerated.Never, ((IProperty)keyProperties[1]).ValueGenerated);
        }

        [Fact]
        public void Identity_not_set_when_primary_key_property_is_non_integer()
        {
            var modelBuilder = CreateInternalModelBuilder();
            var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

            var keyBuilder = entityBuilder.PrimaryKey(new List<string> { "Name" }, ConfigurationSource.Convention);

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
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

            Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
            Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

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

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

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

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, ((IProperty)property).ValueGenerated);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new KeyConvention().Apply(relationshipBuilder));

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

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            var property = keyBuilder.Metadata.Properties.First();

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            var relationshipBuilder = referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Same(relationshipBuilder, new KeyConvention().Apply(relationshipBuilder));

            Assert.Equal(ValueGenerated.Never, ((IProperty)property).ValueGenerated);

            referencedEntityBuilder.RemoveForeignKey(relationshipBuilder.Metadata, ConfigurationSource.Convention);

            Assert.Same(keyBuilder, new KeyConvention().Apply(keyBuilder));

            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        #endregion

        #region ShadowKeys

        [Fact]
        public void Throws_an_exception_when_shadow_key_is_defined_by_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Foo", ConfigurationSource.Convention);
            entityTypeBuilder.HasKey(new List<string> { "Foo" }, ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.ShadowKey("{'Foo'}", typeof(SampleEntity).Name, "{'Foo'}"),
                Assert.Throws<InvalidOperationException>(() => new KeyConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_an_exception_when_shadow_key_is_not_defined_by_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Foo", ConfigurationSource.Convention);
            entityTypeBuilder.HasKey(new List<string> { "Foo" }, ConfigurationSource.DataAnnotation);

            Assert.Same(modelBuilder, new KeyConvention().Apply(modelBuilder));
        }

        [Fact]
        public void Does_not_throw_an_exception_when_key_is_defined_on_shadow_properties_which_are_not_defined_by_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityTypeBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Foo", ConfigurationSource.DataAnnotation);
            entityTypeBuilder.HasKey(new List<string> { "Foo" }, ConfigurationSource.Convention);

            Assert.Same(modelBuilder, new KeyConvention().Apply(modelBuilder));
        }

        [Fact]
        public void Throws_an_exception_when_shadow_key_is_referenced_by_foreign_key()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            referencedEntityBuilder.Property("Foo", ConfigurationSource.Convention);
            var properties = new List<string> { "Foo" };
            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention);

            Assert.Equal(CoreStrings.ReferencedShadowKeyWithoutNavigations(
                "{'Foo'}",
                typeof(SampleEntity).Name,
                "{'Foo'}",
                typeof(ReferencedEntity).Name),
                Assert.Throws<InvalidOperationException>(() => new KeyConvention().Apply(modelBuilder)).Message);
        }

        [Fact]
        public void Does_not_throw_an_exception_when_key_is_referenced_by_foreign_key_and_defined_on_shadow_properties_which_are_not_defined_by_convention()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
            var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

            principalEntityBuilder.Property("Foo", ConfigurationSource.DataAnnotation);
            referencedEntityBuilder.Property("Foo", ConfigurationSource.DataAnnotation);
            var properties = new List<string> { "Foo" };
            referencedEntityBuilder.HasForeignKey(
                principalEntityBuilder,
                referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                ConfigurationSource.Convention)
                .HasPrincipalKey(principalEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
                    ConfigurationSource.Convention);

            Assert.Same(modelBuilder, new KeyConvention().Apply(modelBuilder));
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
            conventions.PrimaryKeySetConventions.Add(keyConvention);

            return new InternalModelBuilder(new Model(conventions));
        }
    }
}
