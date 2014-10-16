// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilderTest
    {
        [Fact]
        public void Can_only_override_lower_source_ConcurrencyToken()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.ConcurrencyToken(true, ConfigurationSource.Convention));
            Assert.True(builder.ConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsConcurrencyToken.Value);

            Assert.False(builder.ConcurrencyToken(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsConcurrencyToken.Value);
        }

        [Fact]
        public void Can_only_override_existing_ConcurrencyToken_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.IsConcurrencyToken = true;

            Assert.False(builder.ConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsConcurrencyToken.Value);

            Assert.True(builder.ConcurrencyToken(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsConcurrencyToken.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_GenerateValuesOnAdd()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.GenerateValuesOnAdd(true, ConfigurationSource.Convention));
            Assert.True(builder.GenerateValuesOnAdd(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);

            Assert.False(builder.GenerateValuesOnAdd(true, ConfigurationSource.Convention));
            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);

            Assert.False(builder.StoreComputed(true, ConfigurationSource.Convention));

            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);
        }

        [Fact]
        public void Can_only_override_existing_GenerateValuesOnAdd_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.ValueGeneration = ValueGeneration.OnAdd;

            Assert.False(builder.GenerateValuesOnAdd(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGeneration.OnAdd, metadata.ValueGeneration.Value);

            Assert.True(builder.GenerateValuesOnAdd(false, ConfigurationSource.Explicit));
            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_StoreComputed()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.StoreComputed(true, ConfigurationSource.Convention));
            Assert.True(builder.StoreComputed(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);

            Assert.False(builder.StoreComputed(true, ConfigurationSource.Convention));
            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);

            Assert.False(builder.GenerateValuesOnAdd(true, ConfigurationSource.Convention));

            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);
        }

        [Fact]
        public void Can_only_override_existing_StoreComputed_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.ValueGeneration = ValueGeneration.OnAddAndUpdate;

            Assert.False(builder.StoreComputed(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(ValueGeneration.OnAddAndUpdate, metadata.ValueGeneration.Value);

            Assert.True(builder.StoreComputed(false, ConfigurationSource.Explicit));
            Assert.Equal(ValueGeneration.None, metadata.ValueGeneration.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_MaxLength()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.MaxLength(1, ConfigurationSource.Convention));
            Assert.True(builder.MaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, metadata.MaxLength.Value);

            Assert.False(builder.MaxLength(1, ConfigurationSource.Convention));
            Assert.Equal(2, metadata.MaxLength.Value);
        }

        [Fact]
        public void Can_only_override_existing_MaxLength_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.MaxLength = 1;

            Assert.False(builder.MaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, metadata.MaxLength.Value);

            Assert.True(builder.MaxLength(2, ConfigurationSource.Explicit));
            Assert.Equal(2, metadata.MaxLength.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_Required()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.Required(true, ConfigurationSource.Convention));
            Assert.True(builder.Required(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsNullable.Value);

            Assert.False(builder.Required(true, ConfigurationSource.Convention));
            Assert.True(metadata.IsNullable.Value);
        }

        [Fact]
        public void Can_only_override_existing_Required_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.IsNullable = false;

            Assert.False(builder.Required(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsNullable.Value);

            Assert.True(builder.Required(false, ConfigurationSource.Explicit));
            Assert.True(metadata.IsNullable.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_Shadow()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.Shadow(true, ConfigurationSource.Convention));
            Assert.True(builder.Shadow(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsShadowProperty);

            Assert.False(builder.Shadow(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsShadowProperty);
        }

        [Fact]
        public void Can_only_override_existing_Shadow_value_explicitly()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.PropertyType, Customer.NameProperty.Name, ConfigurationSource.Explicit);
            var metadata = builder.Metadata;

            Assert.False(builder.Shadow(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsShadowProperty);

            Assert.True(builder.Shadow(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsShadowProperty);
        }

        [Fact]
        public void Can_only_override_lower_source_UseStoreDefault()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.UseStoreDefault(true, ConfigurationSource.Convention));
            Assert.True(builder.UseStoreDefault(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.UseStoreDefault.Value);

            Assert.False(builder.UseStoreDefault(true, ConfigurationSource.Convention));
            Assert.False(metadata.UseStoreDefault.Value);
        }

        [Fact]
        public void Can_only_override_existing_UseStoreDefault_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.UseStoreDefault = true;

            Assert.False(builder.UseStoreDefault(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.UseStoreDefault.Value);

            Assert.True(builder.UseStoreDefault(false, ConfigurationSource.Explicit));
            Assert.False(metadata.UseStoreDefault.Value);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            return entityBuilder.Property(Customer.NameProperty, ConfigurationSource.Convention);
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
