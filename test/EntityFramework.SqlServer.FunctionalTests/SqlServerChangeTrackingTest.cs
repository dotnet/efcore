// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerChangeTrackingTest : ChangeTrackingTestBase<SqlServerNorthwindQueryFixture>
    {
        public SqlServerChangeTrackingTest(SqlServerNorthwindQueryFixture fixture)
            : base(fixture)
        {
        }
    }
}
