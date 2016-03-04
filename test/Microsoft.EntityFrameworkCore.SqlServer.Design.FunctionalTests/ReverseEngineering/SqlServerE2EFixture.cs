// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.FunctionalTests.ReverseEngineering
{
    public class SqlServerE2EFixture
    {
        public SqlServerE2EFixture()
        {
            SqlServerTestStore.CreateDatabase(
                "SqlServerReverseEngineerTestE2E", "ReverseEngineering/E2E.sql", true);
        }
    }
}
