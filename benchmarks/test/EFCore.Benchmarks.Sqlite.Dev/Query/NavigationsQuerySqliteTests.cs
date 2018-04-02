// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Query
{
    public class NavigationsQuerySqliteTests : NavigationsQueryTests
    {
        // Work around issue #10534
        protected override int UnfilteredCount => 586;

        protected override AdventureWorksContextBase CreateContext()
        {
            return AdventureWorksFixture.CreateContext();
        }
    }
}
