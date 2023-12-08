// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocMiscellaneousQuerySqliteTest : AdHocMiscellaneousQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override void Seed2951(Context2951 context)
    {
        context.Database.ExecuteSqlRaw(
"""
CREATE TABLE ZeroKey (Id int);
INSERT INTO ZeroKey VALUES (NULL)
""");
    }
}
