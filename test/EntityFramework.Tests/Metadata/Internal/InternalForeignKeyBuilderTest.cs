// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalForeignKeyBuilderTest
    {
        [Fact]
        public void Can_only_override_lower_source_IsUnique()
        {
            var builder = CreateInternalForeignKeyBuilder();
            var metadata = builder.Metadata;

            Assert.True(builder.IsUnique(true, ConfigurationSource.Convention));
            Assert.True(builder.IsUnique(false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsUnique.Value);

            Assert.False(builder.IsUnique(true, ConfigurationSource.Convention));
            Assert.False(metadata.IsUnique.Value);
        }

        [Fact]
        public void Can_only_override_existing_IsUnique_value_explicitly()
        {
            var builder = CreateInternalForeignKeyBuilder();
            var metadata = builder.Metadata;
            metadata.IsUnique = true;

            Assert.False(builder.IsUnique(false, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsUnique.Value);

            Assert.True(builder.IsUnique(false, ConfigurationSource.Explicit));
            Assert.False(metadata.IsUnique.Value);
        }

        private InternalForeignKeyBuilder CreateInternalForeignKeyBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);
            modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit).Key(new[] { Order.IdProperty }, ConfigurationSource.Explicit);

            return entityBuilder.ForeignKey(typeof(Order), new[] { Customer.IdProperty }, ConfigurationSource.Explicit);
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");

            public int Id { get; set; }
        }
    }
}
