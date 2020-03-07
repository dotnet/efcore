// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public static class AdventureWorksSqlServerFixture
    {
        private static readonly string _connectionString = SqlServerBenchmarkEnvironment.CreateConnectionString("AdventureWorks2014");

        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContextBase CreateContext()
        {
            return new AdventureWorksSqlServerContext(_connectionString);
        }
    }
}
