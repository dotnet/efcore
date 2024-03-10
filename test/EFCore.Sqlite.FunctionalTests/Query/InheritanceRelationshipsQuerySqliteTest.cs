// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class InheritanceRelationshipsQuerySqliteTest(InheritanceRelationshipsQuerySqliteTest.InheritanceRelationshipsQuerySqliteFixture fixture) :
    InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQuerySqliteTest.InheritanceRelationshipsQuerySqliteFixture>(fixture)
{
    public class InheritanceRelationshipsQuerySqliteFixture : InheritanceRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
