// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalPropertyBuilderTest
    {
        [Fact]
        public void Property_added_by_name_is_non_shadow_if_matches_Clr_property()
        {
            var model = new Model();
            var modelBuilder = new InternalModelBuilder(model);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);
            var property = builder.Metadata;

            Assert.Equal(typeof(string), property.ClrType);
            Assert.False(property.IsShadowProperty);
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
            var builder = metadata.Builder;

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetIsConcurrencyTokenConfigurationSource());
            Assert.True(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsConcurrencyToken);

            Assert.True(builder.IsConcurrencyToken(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsConcurrencyToken);
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
            var builder = metadata.Builder;

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
            var builder = metadata.Builder;

            Assert.True(builder.HasMaxLength(1, ConfigurationSource.DataAnnotation));
            Assert.False(builder.HasMaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, metadata.GetMaxLength().Value);

            Assert.True(builder.HasMaxLength(2, ConfigurationSource.Explicit));
            Assert.Equal(2, metadata.GetMaxLength().Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_CustomValueGenerator_factory()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.DataAnnotation));
            Assert.True(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.DataAnnotation));

            Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());

            Assert.False(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.Convention));
            Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());
        }

        [Fact]
        public void Can_only_override_existing_CustomValueGenerator_factory_explicitly()
        {
            ValueGenerator factory(IProperty p, IEntityType e) => new CustomValueGenerator1();

            var metadata = CreateProperty();
            metadata.SetValueGeneratorFactory(factory);
            var builder = metadata.Builder;

            Assert.True(builder.HasValueGenerator(factory, ConfigurationSource.DataAnnotation));
            Assert.False(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.DataAnnotation));

            Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());

            Assert.True(builder.HasValueGenerator((p, e) => new CustomValueGenerator2(), ConfigurationSource.Explicit));
            Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());
        }

        [Fact]
        public void Can_clear_CustomValueGenerator_factory()
        {
            var metadata = CreateProperty();
            var builder = metadata.Builder;

            Assert.True(builder.HasValueGenerator((p, e) => new CustomValueGenerator1(), ConfigurationSource.DataAnnotation));

            Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.True(metadata.RequiresValueGenerator());

            Assert.False(builder.HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>)null, ConfigurationSource.Convention));

            Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.True(metadata.RequiresValueGenerator());

            Assert.True(builder.HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>)null, ConfigurationSource.Explicit));

            Assert.Null(metadata.GetValueGeneratorFactory());
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.False(metadata.RequiresValueGenerator());
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_CustomValueGenerator_type()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.DataAnnotation));
            Assert.True(builder.HasValueGenerator(typeof(CustomValueGenerator2), ConfigurationSource.DataAnnotation));

            Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());

            Assert.False(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.Convention));
            Assert.IsType<CustomValueGenerator2>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.True(metadata.RequiresValueGenerator());
        }

        [Fact]
        public void Can_clear_CustomValueGenerator_type()
        {
            var metadata = CreateProperty();
            var builder = metadata.Builder;

            Assert.True(builder.HasValueGenerator(typeof(CustomValueGenerator1), ConfigurationSource.DataAnnotation));

            Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.True(metadata.RequiresValueGenerator());

            Assert.False(builder.HasValueGenerator((Type)null, ConfigurationSource.Convention));

            Assert.IsType<CustomValueGenerator1>(metadata.GetValueGeneratorFactory()(null, null));
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.True(metadata.RequiresValueGenerator());

            Assert.True(builder.HasValueGenerator((Type)null, ConfigurationSource.Explicit));

            Assert.Null(metadata.GetValueGeneratorFactory());
            Assert.Equal(ValueGenerated.Never, metadata.ValueGenerated);
            Assert.False(metadata.RequiresValueGenerator());
        }

        private class CustomValueGenerator1 : ValueGenerator<string>
        {
            public override string Next(EntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public override bool GeneratesTemporaryValues => false;
        }

        private class CustomValueGenerator2 : ValueGenerator<string>
        {
            public override string Next(EntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public override bool GeneratesTemporaryValues => false;
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_unicode()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsUnicode(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.IsUnicode(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsUnicode().Value);

            Assert.False(builder.IsUnicode(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsUnicode().Value);
        }

        [Fact]
        public void Can_only_override_existing_unicode_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.IsUnicode(true);
            var builder = metadata.Builder;

            Assert.True(builder.IsUnicode(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsUnicode(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsUnicode().Value);

            Assert.True(builder.IsUnicode(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsUnicode().Value);
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
            var builder = metadata.Builder;

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
            var builder = entityBuilder.Property(nameof(Customer.Id), typeof(int), ConfigurationSource.Convention);

            Assert.False(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(
                CoreStrings.CannotBeNullable(nameof(Customer.Id), typeof(Customer).Name, "int"),
                Assert.Throws<InvalidOperationException>(() => builder.IsRequired(false, ConfigurationSource.Explicit)).Message);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_BeforeSaveBehavior()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.BeforeSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
            Assert.True(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertySaveBehavior.Ignore, metadata.BeforeSaveBehavior);

            Assert.False(builder.BeforeSave(PropertySaveBehavior.Save, ConfigurationSource.Convention));
            Assert.Equal(PropertySaveBehavior.Ignore, metadata.BeforeSaveBehavior);
        }

        [Fact]
        public void Can_only_override_existing_BeforeSaveBehavior_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetBeforeSaveBehaviorConfigurationSource());
            metadata.BeforeSaveBehavior = PropertySaveBehavior.Throw;
            var builder = metadata.Builder;

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetBeforeSaveBehaviorConfigurationSource());
            Assert.True(builder.BeforeSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
            Assert.False(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertySaveBehavior.Throw, metadata.BeforeSaveBehavior);

            Assert.True(builder.BeforeSave(PropertySaveBehavior.Ignore, ConfigurationSource.Explicit));
            Assert.Equal(PropertySaveBehavior.Ignore, metadata.BeforeSaveBehavior);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_AfterSaveBehavior()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
            Assert.True(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertySaveBehavior.Ignore, metadata.AfterSaveBehavior);

            Assert.False(builder.AfterSave(PropertySaveBehavior.Save, ConfigurationSource.Convention));
            Assert.Equal(PropertySaveBehavior.Ignore, metadata.AfterSaveBehavior);
        }

        [Fact]
        public void Can_only_override_existing_AfterSaveBehavior_value_explicitly()
        {
            var metadata = CreateProperty();
            Assert.Null(metadata.GetAfterSaveBehaviorConfigurationSource());
            metadata.AfterSaveBehavior = PropertySaveBehavior.Throw;
            var builder = metadata.Builder;

            Assert.Equal(ConfigurationSource.Explicit, metadata.GetAfterSaveBehaviorConfigurationSource());
            Assert.True(builder.AfterSave(PropertySaveBehavior.Throw, ConfigurationSource.DataAnnotation));
            Assert.False(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertySaveBehavior.Throw, metadata.AfterSaveBehavior);

            Assert.True(builder.AfterSave(PropertySaveBehavior.Ignore, ConfigurationSource.Explicit));
            Assert.Equal(PropertySaveBehavior.Ignore, metadata.AfterSaveBehavior);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            return entityBuilder.Property(Customer.NameProperty, ConfigurationSource.Convention);
        }

        private Property CreateProperty() => CreateInternalPropertyBuilder().Metadata;

        private class Customer
        {
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
