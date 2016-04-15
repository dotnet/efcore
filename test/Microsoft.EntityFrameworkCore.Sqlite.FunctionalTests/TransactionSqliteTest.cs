// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class TransactionSqliteTest : TransactionTestBase<SqliteTestStore, TransactionSqliteFixture>
    {
        public TransactionSqliteTest(TransactionSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported => false;
    }
}
