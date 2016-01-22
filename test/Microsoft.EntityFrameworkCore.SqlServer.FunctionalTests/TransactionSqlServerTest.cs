// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class TransactionSqlServerTest : TransactionTestBase<SqlServerTestStore, TransactionSqlServerFixture>
    {
        public TransactionSqlServerTest(TransactionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported => true;
    }
}
