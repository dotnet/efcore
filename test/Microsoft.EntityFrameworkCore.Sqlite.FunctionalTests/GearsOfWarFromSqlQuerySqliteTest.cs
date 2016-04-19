// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class GearsOfWarFromSqlQuerySqliteTest : GearsOfWarFromSqlQueryTestBase<SqliteTestStore, GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarFromSqlQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
        }
    }
}
