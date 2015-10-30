// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Metadata.Conventions;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilderTest
    {
        [Fact]
        public void Can_only_override_lower_or_equal_source_ClrType()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ClrType(typeof(int), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(int), metadata.ClrType);

            Assert.True(builder.ClrType(typeof(string), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(string), metadata.ClrType);

            Assert.False(builder.ClrType(typeof(int), ConfigurationSource.Convention));

            Assert.Equal(typeof(string), metadata.ClrType);

            Assert.True(builder.ClrType(typeof(string), ConfigurationSource.Convention));
        }

        [Fact]
        public void Can_only_override_existing_ClrType_value_explicitly()
        {
            var model = new Model();
            model.AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty);
            var modelBuilder = new InternalModelBuilder(model);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);

            Assert.True(builder.ClrType(typeof(string), ConfigurationSource.DataAnnotation));
            Assert.False(builder.ClrType(typeof(int), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(string), builder.Metadata.ClrType);

            Assert.True(builder.ClrType(typeof(int), ConfigurationSource.Explicit));
            Assert.Equal(typeof(int), builder.Metadata.ClrType);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ConcurrencyToken()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsConcurrencyToken.Value);

            Assert.False(builder.IsConcurrencyToken(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsConcurrencyToken.Value);
        }

        [Fact]
        public void Can_only_override_existing_ConcurrencyToken_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.IsConcurrencyToken = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.IsConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsConcurrencyToken.Value);

            Assert.True(builder.IsConcurrencyToken(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsConcurrencyToken.Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_UseValueGenerator()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.UseValueGenerator(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.UseValueGenerator(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(false, metadata.RequiresValueGenerator);

            Assert.False(builder.UseValueGenerator(true, ConfigurationSource.Convention));
            Assert.Equal(false, metadata.RequiresValueGenerator);
        }

        [Fact]
        public void Can_only_override_existing_UseValueGenerator_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.RequiresValueGenerator = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.UseValueGenerator(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.UseValueGenerator(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(true, metadata.RequiresValueGenerator);

            Assert.True(builder.UseValueGenerator(false, ConfigurationSource.Explicit));
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
            metadata.ValueGenerated = ValueGenerated.OnAddOrUpdate;
            var builder = CreateInternalPropertyBuilder(metadata);

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

            Assert.True(metadata.IsNullable.Value);

            Assert.False(builder.IsRequired(true, ConfigurationSource.Convention));
            Assert.True(metadata.IsNullable.Value);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.IsNullable = false;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.IsRequired(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.IsRequired(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsNullable.Value);

            Assert.True(builder.IsRequired(false, ConfigurationSource.Explicit));
            Assert.True(metadata.IsNullable.Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ReadOnlyAfterSave()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ReadOnlyAfterSave(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.ReadOnlyAfterSave(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyAfterSave.Value);

            Assert.False(builder.ReadOnlyAfterSave(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsReadOnlyAfterSave.Value);
        }

        [Fact]
        public void Can_only_override_existing_ReadOnlyAfterSave_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.IsReadOnlyAfterSave = false;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.ReadOnlyAfterSave(false, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ReadOnlyAfterSave(true, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyAfterSave.Value);

            Assert.True(builder.ReadOnlyAfterSave(true, ConfigurationSource.Explicit));
            Assert.True(metadata.IsReadOnlyAfterSave.Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_ReadOnlyBeforeSave()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ReadOnlyBeforeSave(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.ReadOnlyBeforeSave(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsReadOnlyBeforeSave.Value);

            Assert.False(builder.ReadOnlyBeforeSave(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsReadOnlyBeforeSave.Value);
        }

        [Fact]
        public void Can_only_override_existing_ReadOnlyBeforeSave_value_explicitly()
        {
            var metadata = CreateProperty();
            metadata.IsReadOnlyBeforeSave = true;
            var builder = CreateInternalPropertyBuilder(metadata);

            Assert.True(builder.ReadOnlyBeforeSave(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ReadOnlyBeforeSave(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsReadOnlyBeforeSave.Value);

            Assert.True(builder.ReadOnlyBeforeSave(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsReadOnlyBeforeSave.Value);
        }

        [Fact]
        public void Can_only_override_lower_or_equal_source_Shadow()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.Shadow(true, ConfigurationSource.DataAnnotation));
            Assert.True(builder.Shadow(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsShadowProperty);

            Assert.False(builder.Shadow(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsShadowProperty);
        }

        [Fact]
        public void Can_only_override_existing_Shadow_value_explicitly()
        {
            var model = new Model();
            model.AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty);
            var modelBuilder = new InternalModelBuilder(model);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.Name, ConfigurationSource.Convention);

            Assert.True(builder.Shadow(false, ConfigurationSource.DataAnnotation));
            Assert.False(builder.Shadow(true, ConfigurationSource.DataAnnotation));

            Assert.False(builder.Metadata.IsShadowProperty);

            Assert.True(builder.Shadow(true, ConfigurationSource.Explicit));
            Assert.True(builder.Metadata.IsShadowProperty);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            return entityBuilder.Property(Customer.NameProperty, ConfigurationSource.Convention);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder(Property property)
        {
            var modelBuilder = new InternalModelBuilder(property.DeclaringEntityType.Model);
            var entityBuilder = modelBuilder.Entity(property.DeclaringEntityType.ClrType, ConfigurationSource.Convention);
            return entityBuilder.Property(property.Name, ConfigurationSource.Convention);
        }

        private Property CreateProperty() => new Model().AddEntityType(typeof(Customer)).AddProperty(Customer.NameProperty);

        private class Customer
        {
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
