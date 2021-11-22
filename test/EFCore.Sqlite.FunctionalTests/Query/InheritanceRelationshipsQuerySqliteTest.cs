// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class InheritanceRelationshipsQuerySqliteTest :
    InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQuerySqliteTest.InheritanceRelationshipsQuerySqliteFixture>
{
    public InheritanceRelationshipsQuerySqliteTest(InheritanceRelationshipsQuerySqliteFixture fixture)
        : base(fixture)
    {
    }

    public class InheritanceRelationshipsQuerySqliteFixture : InheritanceRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
