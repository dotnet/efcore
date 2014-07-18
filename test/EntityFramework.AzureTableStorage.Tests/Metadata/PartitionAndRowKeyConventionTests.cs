// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class PartitionAndRowKeyConventionTests
    {
        private readonly PartitionKeyAndRowKeyConvention _convention = new PartitionKeyAndRowKeyConvention();

        [Fact]
        public void It_does_not_add_pk_and_rk_props()
        {
            var entityType = new EntityType("John Maynard");

            _convention.Apply(entityType);

            Assert.Equal(0, entityType.Properties.Count);
            Assert.Null(entityType.TryGetKey());
        }

        [Theory]
        [InlineData("PartitionKey")]
        [InlineData("RowKey")]
        public void It_requires_both_properties(string onlyProp)
        {
            var entityType = new EntityType("John Maynard");
            entityType.AddProperty(onlyProp, typeof(string));

            _convention.Apply(entityType);

            Assert.Equal(1, entityType.Properties.Count);
            Assert.Null(entityType.TryGetKey());
        }

        [Fact]
        public void It_adds_composite_key()
        {
            var entityType = new EntityType("John Maynard");
            entityType.AddProperty("PartitionKey", typeof(string));
            entityType.AddProperty("RowKey", typeof(string));

            _convention.Apply(entityType);

            var key = entityType.GetKey();
            Assert.Equal(2, key.Properties.Count);
            Assert.Contains("PartitionKey", key.Properties.Select(p => p.ColumnName()));
            Assert.Contains("RowKey", key.Properties.Select(p => p.ColumnName()));
        }
    }
}
