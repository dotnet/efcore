// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Extensions
{
    public class EntityBuilderExtensionsTests
    {
        [Fact]
        public void It_creates_composite_key()
        {
            var model = new Model();
            var builder = new BasicModelBuilder(model);

            builder.Entity<PocoTestType>()
                .ForAzureTableStorage().PartitionAndRowKey(s => s.BigCount, s => s.IsEnchanted);

            var key = model.EntityTypes.First().GetPrimaryKey();
            Assert.Equal(2, key.Properties.Count);
            Assert.Equal("BigCount", key.Properties.First(s => s.AzureTableStorage().Column == "PartitionKey").Name);
            Assert.Equal("IsEnchanted", key.Properties.First(s => s.AzureTableStorage().Column == "RowKey").Name);
        }
    }
}
