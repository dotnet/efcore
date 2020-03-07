// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public class AdventureWorksSqliteContext : AdventureWorksContextBase
    {
        private readonly string _connectionString;

        public AdventureWorksSqliteContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}
