// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class GearsOfWarQuerySqliteFixture : GearsOfWarQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
