// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class AdventureWorksSqlServerContext : AdventureWorksContextBase
    {
        private readonly string _connectionString;

        public AdventureWorksSqlServerContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}
