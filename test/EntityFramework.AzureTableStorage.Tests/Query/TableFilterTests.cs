// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Query
{
    public class TableFilterTests
    {
        [Fact]
        public void It_always_uses_string_comparison_for_row_key()
        {
            var filter = new TableFilter.ConstantTableFilter("RowKey", FilterComparisonOperator.Equal, Expression.Constant(123456));
            Assert.Equal("RowKey eq '123456'", filter.ToString());
        }

        [Fact]
        public void It_always_uses_string_comparison_for_partition_key()
        {
            var filter = new TableFilter.ConstantTableFilter("PartitionKey", FilterComparisonOperator.Equal, Expression.Constant(123456));
            Assert.Equal("PartitionKey eq '123456'", filter.ToString());
        }

        [Fact]
        public void Int_literals()
        {
            var filter = new TableFilter.ConstantTableFilter("Count", FilterComparisonOperator.Equal, Expression.Constant(123456));
            Assert.Equal("Count eq 123456", filter.ToString());
        }

        [Fact]
        public void Empty_for_nulls()
        {
            var filter = new TableFilter.ConstantTableFilter("Name", FilterComparisonOperator.Equal, Expression.Constant(null));
            Assert.Equal(string.Empty, filter.ToString());
        }
    }
}
