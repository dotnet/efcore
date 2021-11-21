// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

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
