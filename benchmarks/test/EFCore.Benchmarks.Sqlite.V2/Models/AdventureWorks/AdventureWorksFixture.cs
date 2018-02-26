// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public static class AdventureWorksFixture
    {
        // TODO: No database for Sqlite
        private static string _connectionString = $"DataSource=AdventureWorks2014;";

        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContextBase CreateContext()
        {
            return new AdventureWorksContext(_connectionString);
        }
    }
}
