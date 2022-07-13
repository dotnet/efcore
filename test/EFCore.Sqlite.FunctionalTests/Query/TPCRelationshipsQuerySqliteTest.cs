// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

internal class TPCRelationshipsQuerySqliteTest :
    TPCRelationshipsQueryTestBase<TPCRelationshipsQuerySqliteTest.TPCRelationshipsQuerySqliteFixture>
{
    public TPCRelationshipsQuerySqliteTest(TPCRelationshipsQuerySqliteFixture fixture)
        : base(fixture)
    {
    }

    public class TPCRelationshipsQuerySqliteFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
