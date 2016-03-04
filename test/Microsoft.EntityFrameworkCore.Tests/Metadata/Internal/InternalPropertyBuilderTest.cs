// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class InternalPropertyBuilderTest
    {
        [Fact]
        public void Can_only_override_lower_or_equal_source_ClrType()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.HasClrType(typeof(int), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(int), metadata.ClrType);

            Assert.True(builder.HasClrType(typeof(string), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(string), metadata.ClrType);

            Assert.False(builder.HasClrType(typeof(int), ConfigurationSource.Convention));

            Assert.Equal(typeof(string), metadata.ClrType);

            Assert.True(builder.HasClrType(typeof(string), ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_override_existing_ClrType_value_explicitly()
        {
            var model = new Model();
            model.AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty.Name);
            var modelBuilder = new InternalModelBuilder(model);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);
            Assert.Null(builder.Metadata.GetClrTypeConfigurationSource());

            builder.Metadata.ClrType = typeof(string);

            Assert.Equal(ConfigurationSource.Explicit, builder.Metadata.GetClrTypeConfigurationSource());
            Assert.True(builder.HasClrType(typeof(string), ConfigurationSource.DataAnnotation));
            Assert.False(builder.HasClrType(typeof(int), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(string), builder.Metadata.ClrType);

            Assert.True(builder.HasClrType(typeof(int), ConfigurationSource.Explicit));
            Assert.Equal(typeof(int), builder.Metadata.ClrType);
        }

        [Fact]
        public void Property_added_by_name_is_shadow_even_if_matches_Clr_type()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);
            var property = builder.Metadata;

            Assert.Equal(typeof(string), property.ClrType);
            Assert.True(property.IsShadowProperty);
            Assert.Null(property.GetClrTypeConfigurationSource());
            Assert.Null(property.GetIsShadowPropertyConfigurationSource());
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ConcurrencyToken()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsConcurrencyToken);

            Assert.False(builder.IsConcurrencyToken(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsConcurrencyToken);
        }

        [Fact]
        public void Can_only_override_existing_ConcurrencyToken_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetIsConcurrencyTokenConfigurationSource());
            metadata.IsConcurrencyToken = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsConcurrencyTokenConfigurationSource());
            Assert.True(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsConcurrencyToken);

            Assert.True(builder.IsConcurrencyToken(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsConcurrencyToken);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_UseValueGenerator()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.RequiresValueGenerator(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.RequiresValueGenerator(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(false, metadata.RequiresValueGenerator);

            Assert.False(builder.RequiresValueGenerator(true, ConfigurationSource.Convention));
            Assert.Equal(false, metadata.RequiresValueGenerator);
        }

        [Fact]
        public void Can_only_override_existing_RequiresValueGenerator_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetRequiresValueGeneratorConfigurationSource());
            metadata.RequiresValueGenerator = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetRequiresValueGeneratorConfigurationSource());
            Assert.True(builder.RequiresValueGenerator(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.RequiresValueGenerator(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(true, metadata.RequiresValueGenerator);

            Assert.True(builder.RequiresValueGenerator(false, ConfigurationSource.Explicit));
            Assert.Equal(false, metadata.RequiresValueGenerator);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ValueGenerated()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation));
            Assert.True(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);

            Assert.False(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        }

        [Fact]
        public void Can_only_override_existing_ValueGenerated_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetValueGeneratedConfigurationSource());
            metadata.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetValueGeneratedConfigurationSource());
            Assert.True(builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGenerated.OnAddOrUpdate, metadata.ValueGenerated);

            Assert.True(builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_MaxLength()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.HasMaxLength(1, ConfigurationSource.DataAnnotation));
            Assert.True(builder.HasMaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, metadata.GetMaxLength().Value);

            Assert.False(builder.HasMaxLength(1, ConfigurationSource.Convention));
            Assert.Equal(2, metadata.GetMaxLength().Value);
        }

        [Fact]
        public void Can_only_override_existing_MaxLength_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.SetMaxLength(1);
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.HasMaxLength(1, ConfigurationSource.DataAnnotation));
            Assert.False(builder.HasMaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, metadata.GetMaxLength().Value);

            Assert.True(builder.HasMaxLength(2, ConfigurationSource.Explicit));
            Assert.Equal(2, metadata.GetMaxLength().Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_Required()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsRequired(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsNullable);

            Assert.False(builder.IsRequired(true, ConfigurationSource.Convention));
            Assert.True(metadata.IsNullable);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetIsNullableConfigurationSource());
            metadata.IsNullable = false;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsNullableConfigurationSource());
            Assert.True(builder.IsRequired(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsNullable);

            Assert.True(builder.IsRequired(false, ConfigurationSource.Explicit));
            Assert.True(metadata.IsNullable);
        }

        [Fact]
        public void Cannot_set_required_to_false_if_nonnullable()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            var builder = entityBuilder.Property(nameof(Customer.Id), ConfigurationSource.Convention);
            builder.HasClrType(typeof(int), ConfigurationSource.Convention);

            Assert.False(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(CoreStrings.CannotBeNullable(nameof(Customer.Id), typeof(Customer).Name, typeof(int).Name),
                Assert.Throws<InvalidOperationException>(() => builder.IsRequired(false, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ReadOnlyAfterSave()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ReadOnlyAfterSave(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.ReadOnlyAfterSave(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyAfterSave);

            Assert.False(builder.ReadOnlyAfterSave(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsReadOnlyAfterSave);
        }

        [Fact]
        public void Can_only_override_existing_ReadOnlyAfterSave_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetIsReadOnlyAfterSaveConfigurationSource());
            metadata.IsReadOnlyAfterSave = false;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsReadOnlyAfterSaveConfigurationSource());
            Assert.True(builder.ReadOnlyAfterSave(false, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ReadOnlyAfterSave(true, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyAfterSave);

            Assert.True(builder.ReadOnlyAfterSave(true, ConfigurationSource.Explicit));
            Assert.True(metadata.IsReadOnlyAfterSave);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ReadOnlyBeforeSave()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ReadOnlyBeforeSave(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.ReadOnlyBeforeSave(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyBeforeSave);

            Assert.False(builder.ReadOnlyBeforeSave(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Can_only_override_existing_ReadOnlyBeforeSave_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetIsReadOnlyBeforeSaveConfigurationSource());
            metadata.IsReadOnlyBeforeSave = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsReadOnlyBeforeSaveConfigurationSource());
            Assert.True(builder.ReadOnlyBeforeSave(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ReadOnlyBeforeSave(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsReadOnlyBeforeSave);

            Assert.True(builder.ReadOnlyBeforeSave(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_Shadow()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsShadow(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.IsShadow(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsShadowProperty);

            Assert.False(builder.IsShadow(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsShadowProperty);
        }

        [Fact]
        public void Can_only_override_existing_Shadow_value_explicitly()
        {
            var model = new Model();
            var metadata = model.AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty.Name);
            Assert.Null(metadata.GetIsShadowPropertyConfigurationSource());
            metadata.IsShadowProperty = false;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsShadowPropertyConfigurationSource());
            Assert.True(builder.IsShadow(false, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsShadow(true, ConfigurationSource.DataAnnotation));

            Assert.False(builder.Metadata.IsShadowProperty);

            Assert.True(builder.IsShadow(true, ConfigurationSource.Explicit));
            Assert.True(builder.Metadata.IsShadowProperty);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            return entityBuilder.Property(Customer.NameProperty, ConfigurationSource.Convention);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder(Property property) => property.Builder;

        private Property CreateProperty() => new Model().AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty);

        private class Customer
        {
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
