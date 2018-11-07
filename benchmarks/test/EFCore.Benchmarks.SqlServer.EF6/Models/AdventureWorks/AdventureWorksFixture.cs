// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public static class AdventureWorksFixture
    {
        public static string ConnectionString { get; } = $"{BenchmarkConfig.Instance.BenchmarkDatabase}Database=AdventureWorks2014;";

        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContext CreateContext()
        {
            return new AdventureWorksContext(ConnectionString);
        }
    }
}
