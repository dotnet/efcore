// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class SqlBatchBuilderTest
    {
        [Fact]
        public void SqlBatchBuilder_correctly_groups_multiple_statements_into_one_batch()
        {
            var batchBuilder = new SqlBatchBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndBatch();

            Assert.Equal(1, batchBuilder.SqlBatches.Count);
            Assert.Equal(
                @"Statement1
Statement2
Statement3
", batchBuilder.SqlBatches[0].Sql);
        }

        [Fact]
        public void SqlBatchBuilder_correctly_produces_multiple_batches()
        {
            var batchBuilder = new SqlBatchBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.EndBatch();
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndBatch();
            batchBuilder.AppendLine("Statement4");
            batchBuilder.AppendLine("Statement5");
            batchBuilder.AppendLine("Statement6");
            batchBuilder.EndBatch();

            Assert.Equal(3, batchBuilder.SqlBatches.Count);

            Assert.Equal(
                @"Statement1
", batchBuilder.SqlBatches[0].Sql);

            Assert.Equal(
                @"Statement2
Statement3
", batchBuilder.SqlBatches[1].Sql);

            Assert.Equal(
                @"Statement4
Statement5
Statement6
", batchBuilder.SqlBatches[2].Sql);
        }

        [Fact]
        public void SqlBatchBuilder_ignores_empty_batches()
        {
            var batchBuilder = new SqlBatchBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.EndBatch();
            batchBuilder.EndBatch();
            batchBuilder.EndBatch();
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndBatch();
            batchBuilder.EndBatch();

            Assert.Equal(2, batchBuilder.SqlBatches.Count);

            Assert.Equal(
                @"Statement1
", batchBuilder.SqlBatches[0].Sql);

            Assert.Equal(
                @"Statement2
Statement3
", batchBuilder.SqlBatches[1].Sql);
        }

        [Fact]
        public void SqlBatchBuilder_correctly_splits_statements_to_multiple_batches_when_transaction_is_suppressed()
        {
            var batchBuilder = new SqlBatchBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3", suppressTransaction: true);
            batchBuilder.AppendLine("Statement4");
            batchBuilder.AppendLine("Statement5");
            batchBuilder.AppendLine("Statement6", suppressTransaction: true);
            batchBuilder.AppendLine("Statement7", suppressTransaction: true);
            batchBuilder.EndBatch();

            Assert.Equal(2, batchBuilder.SqlBatches.Count);
            Assert.False(batchBuilder.SqlBatches[0].SuppressTransaction);
            Assert.Equal(
                @"Statement1
Statement2
", batchBuilder.SqlBatches[0].Sql);

            Assert.True(batchBuilder.SqlBatches[1].SuppressTransaction);
            Assert.Equal(
                @"Statement3
Statement4
Statement5
Statement6
Statement7
", batchBuilder.SqlBatches[1].Sql);
        }
    }
}
