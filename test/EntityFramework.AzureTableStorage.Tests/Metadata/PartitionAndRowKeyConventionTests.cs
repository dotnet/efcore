// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Metadata
{
    public class PartitionAndRowKeyConventionTests
    {
        private readonly PartitionKeyAndRowKeyConvention _convention = new PartitionKeyAndRowKeyConvention();

        [Fact]
        public void It_does_not_add_pk_and_rk_props()
        {
            var entityBuilder = CreateInternalEntityBuilder();

            _convention.Apply(entityBuilder);

            Assert.Equal(0, entityBuilder.Metadata.Properties.Count);
            Assert.Null(entityBuilder.Metadata.TryGetPrimaryKey());
        }

        [Theory]
        [InlineData("PartitionKey")]
        [InlineData("RowKey")]
        public void It_requires_both_properties(string onlyProp)
        {
            var entityBuilder = CreateInternalEntityBuilder();
            entityBuilder.Property(typeof(string), onlyProp, ConfigurationSource.Convention);

            _convention.Apply(entityBuilder);

            Assert.Equal(1, entityBuilder.Metadata.Properties.Count);
            Assert.Null(entityBuilder.Metadata.TryGetPrimaryKey());
        }

        [Fact]
        public void It_adds_composite_key()
        {
            var entityBuilder = CreateInternalEntityBuilder();
            entityBuilder.Property(typeof(string), "PartitionKey", ConfigurationSource.Convention);
            entityBuilder.Property(typeof(string), "RowKey", ConfigurationSource.Convention);

            _convention.Apply(entityBuilder);

            var key = entityBuilder.Metadata.GetPrimaryKey();
            Assert.Equal(2, key.Properties.Count);
            Assert.Contains("PartitionKey", key.Properties.Select(p => p.AzureTableStorage().Column));
            Assert.Contains("RowKey", key.Properties.Select(p => p.AzureTableStorage().Column));
        }

        private InternalEntityBuilder CreateInternalEntityBuilder()
        {
            var modelBuilder = new InternalModelBuilder(new Model(), null);
            return modelBuilder.Entity("John Maynard", ConfigurationSource.Convention);
        }
    }
}
