// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class TransactionSqlServerTest : TransactionTestBase<SqlServerTestStore, TransactionSqlServerFixture>
    {
        public TransactionSqlServerTest(TransactionSqlServerFixture fixture)
            : base(fixture)
        {
            TestSqlServerRetryingExecutionStrategy.Suspended = true;
        }

        protected override bool SnapshotSupported => true;

        [Theory(Skip = "Test is flaky on CI")]
        public override void SaveChanges_uses_explicit_transaction_and_does_not_rollback_on_failure(bool autoTransaction)
        {
            base.SaveChanges_uses_explicit_transaction_and_does_not_rollback_on_failure(autoTransaction);
        }

        [Theory(Skip = "Test is flaky on CI")]
        public override Task RelationalTransaction_can_be_commited(bool autoTransaction)
        {
            return base.RelationalTransaction_can_be_commited(autoTransaction);
        }

        public override void Dispose()
        {
            base.Dispose();
            TestSqlServerRetryingExecutionStrategy.Suspended = false;
        }
    }
}
