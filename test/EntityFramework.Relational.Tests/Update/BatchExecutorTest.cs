// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Moq.Protected;
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
        public async Task ExecuteAsync_delegates()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();
            
            var dbTransactionMock = new Mock<DbTransaction>();
            var mockDbConnection = new Mock<DbConnection>();
            mockDbConnection
                .Protected()
                .Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>())
                .Returns(dbTransactionMock.Object);
            var mockRelationalConnection = new Mock<RelationalConnection>();
            mockRelationalConnection.Setup(m => m.DbConnection).Returns(mockDbConnection.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutor(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            dbTransactionMock.Verify(t => t.Commit());
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(It.IsAny<DbTransaction>(), relationalTypeMapper, cancellationToken));
        }
    }
}
