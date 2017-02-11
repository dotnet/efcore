// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NorthwindDbFunctionSqlServerFixture : NorthwindQuerySqlServerFixture
    {
        public NorthwindDbFunctionSqlServerFixture()
        {
            SqlServerTestStore.ExecuteScript(DatabaseName, "DbFunctions.sql");
        }

        public override NorthwindContext CreateContext(
                QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            => new SqlServerDbFunctionsNorthwindContext(Options, queryTrackingBehavior);
    }
}
