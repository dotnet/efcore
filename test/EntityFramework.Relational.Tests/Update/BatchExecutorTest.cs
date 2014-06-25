// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public void Constructor_checks_arguments()
        {
            Assert.Equal(
                "typeMapper",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    new BatchExecutor(null)).ParamName);
        }

        [Fact]
        public async Task ExecuteAsync_checks_arguments()
        {
            var batchExecutor = new BatchExecutor(new RelationalTypeMapper());

            Assert.Equal(
                "commandBatches",
                // ReSharper disable once AssignNullToNotNullAttribute
                (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    batchExecutor.ExecuteAsync(null, new Mock<RelationalConnection>().Object))).ParamName);

            Assert.Equal(
                "connection",
                // ReSharper disable once AssignNullToNotNullAttribute
                (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    batchExecutor.ExecuteAsync(Enumerable.Empty<ModificationCommandBatch>(), null))).ParamName);
        }

        [Fact]
        public async Task ExecuteAsync_delegates()
        {
            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutor(relationalTypeMapper);

            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();
            var mockRelationalConnection = new Mock<RelationalConnection>();
            var cancellationToken = new CancellationTokenSource().Token;

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(mockRelationalConnection.Object, relationalTypeMapper, cancellationToken));
        }
    }
}
