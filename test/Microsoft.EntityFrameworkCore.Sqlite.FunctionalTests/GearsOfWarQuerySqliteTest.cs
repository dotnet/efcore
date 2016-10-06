// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class GearsOfWarQuerySqliteTest : GearsOfWarQueryTestBase<SqliteTestStore, GearsOfWarQuerySqliteFixture>
    {
        public GearsOfWarQuerySqliteTest(GearsOfWarQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            // TODO: #6702
            //base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            // TODO: #6702
            //base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            // TODO: #6702
            //base.Select_Where_Navigation_Equals_Navigation();
        }
    }
}
