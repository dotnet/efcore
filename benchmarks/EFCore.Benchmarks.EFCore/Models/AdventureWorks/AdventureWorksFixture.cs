// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks.TestHelpers;

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.AdventureWorks
{
    public class AdventureWorksFixture : AdventureWorksFixtureBase
    {
        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContext CreateContext()
            => new AdventureWorksContext(ConnectionString);
    }
}
