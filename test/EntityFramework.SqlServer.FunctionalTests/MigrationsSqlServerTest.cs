// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MigrationsSqlServerTest : MigrationsTestBase<MigrationsSqlServerFixture>
    {
        public MigrationsSqlServerTest(MigrationsSqlServerFixture fixture)
            : base(fixture)
        {
        }
    }
}
