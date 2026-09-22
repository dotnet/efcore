// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

internal class TPCRelationshipsQuerySqliteTest(TPCRelationshipsQuerySqliteTest.TPCRelationshipsQuerySqliteFixture fixture) :
    TPCRelationshipsQueryTestBase<TPCRelationshipsQuerySqliteTest.TPCRelationshipsQuerySqliteFixture>(fixture)
{
    public class TPCRelationshipsQuerySqliteFixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
