// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class OptimisticConcurrencySqlServerTest : OptimisticConcurrencyTestBase<SqlServerTestStore, F1SqlServerFixture>
    {
        public OptimisticConcurrencySqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
        }
    }
}
