// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
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

            Assert.True(builder.ConcurrencyToken(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.ConcurrencyToken(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsConcurrencyToken.Value);

            Assert.True(builder.ConcurrencyToken(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsConcurrencyToken.Value);
        }

        [Fact]
        public void Can_only_override_lower_source_GenerateValueOnAdd()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.GenerateValueOnAdd(true, ConfigurationSource.Convention));
            Assert.True(builder.GenerateValueOnAdd(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(false, metadata.IsValueGeneratedOnAdd);

            Assert.False(builder.GenerateValueOnAdd(true, ConfigurationSource.Convention));
            Assert.Equal(false, metadata.IsValueGeneratedOnAdd);
        }

        [Fact]
        public void Can_only_override_existing_GenerateValueOnAdd_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.IsValueGeneratedOnAdd = true;

            Assert.True(builder.GenerateValueOnAdd(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.GenerateValueOnAdd(false, ConfigurationSource.DataAnnotation));

            Assert.Equal(true, metadata.IsValueGeneratedOnAdd);

            Assert.True(builder.GenerateValueOnAdd(false, ConfigurationSource.Explicit));
            Assert.Equal(false, metadata.IsValueGeneratedOnAdd);
        }

        [Fact]
        public void Can_only_override_lower_source_StoreGeneratedPattern()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.StoreGeneratedPattern(StoreGeneratedPattern.Computed, ConfigurationSource.Convention));
            Assert.True(builder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.DataAnnotation));

            Assert.Equal(StoreGeneratedPattern.None, metadata.StoreGeneratedPattern);

            Assert.False(builder.StoreGeneratedPattern(StoreGeneratedPattern.Computed, ConfigurationSource.Convention));
            Assert.Equal(StoreGeneratedPattern.None, metadata.StoreGeneratedPattern);
        }

        [Fact]
        public void Can_only_override_existing_StoreGeneratedPattern_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.StoreGeneratedPattern = StoreGeneratedPattern.Computed;

            Assert.True(builder.StoreGeneratedPattern(StoreGeneratedPattern.Computed, ConfigurationSource.DataAnnotation));
            Assert.False(builder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.DataAnnotation));

            Assert.Equal(StoreGeneratedPattern.Computed, metadata.StoreGeneratedPattern);

            Assert.True(builder.StoreGeneratedPattern(StoreGeneratedPattern.None, ConfigurationSource.Explicit));
            Assert.Equal(StoreGeneratedPattern.None, metadata.StoreGeneratedPattern);
        }

        [Fact]
        public void Can_only_override_lower_source_MaxLength()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.MaxLength(1, ConfigurationSource.Convention));
            Assert.True(builder.MaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(2, metadata.GetMaxLength().Value);

            Assert.False(builder.MaxLength(1, ConfigurationSource.Convention));
            Assert.Equal(2, metadata.GetMaxLength().Value);
        }

        [Fact]
        public void Can_only_override_existing_MaxLength_value_explicitly()
        {
            var builder = CreateInternalPropertyBuilder();
            var metadata = builder.Metadata;
            metadata.SetMaxLength(1);

            Assert.True(builder.MaxLength(1, ConfigurationSource.DataAnnotation));
            Assert.False(builder.MaxLength(2, ConfigurationSource.DataAnnotation));

            Assert.Equal(1, metadata.GetMaxLength().Value);

            Assert.True(builder.MaxLength(2, ConfigurationSource.Explicit));
            Assert.Equal(2, metadata.GetMaxLength().Value);
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

            Assert.True(builder.Required(true, ConfigurationSource.DataAnnotation));
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
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            var builder = entityBuilder.Property(Customer.NameProperty.PropertyType, Customer.NameProperty.Name, ConfigurationSource.Explicit);
            var metadata = builder.Metadata;

            Assert.True(builder.Shadow(true, ConfigurationSource.DataAnnotation));
            Assert.False(builder.Shadow(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsShadowProperty);

            Assert.True(builder.Shadow(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsShadowProperty);
        }

        private InternalPropertyBuilder CreateInternalPropertyBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), new ConventionSet());
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
