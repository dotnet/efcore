// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
