// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.FunctionalTests;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqLiteTransactionTest : TransactionTestBase<SqLiteTestStore, SqLiteTransactionFixture>
    {
        public SqLiteTransactionTest(SqLiteTransactionFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported
        {
            get { return false; }
        }
    }
}
