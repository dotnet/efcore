// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPTRelationshipsQuerySqliteTest(TPTRelationshipsQuerySqliteTest.TPTRelationshipsQuerySqliteFixture fixture) :
    TPTRelationshipsQueryTestBase<TPTRelationshipsQuerySqliteTest.TPTRelationshipsQuerySqliteFixture>(fixture)
{
    public class TPTRelationshipsQuerySqliteFixture : TPTRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
