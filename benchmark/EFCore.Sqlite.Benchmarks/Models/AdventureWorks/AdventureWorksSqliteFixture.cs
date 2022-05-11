// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public static class AdventureWorksSqliteFixture
    {
        private static readonly string _baseDirectory
            = Path.GetDirectoryName(typeof(AdventureWorksSqliteFixture).Assembly.Location);

        private static readonly string _connectionString
            = $"Data Source={Path.Combine(_baseDirectory, "AdventureWorks2014.db")}";

        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContextBase CreateContext()
        {
            return new AdventureWorksSqliteContext(_connectionString);
        }
    }
}
