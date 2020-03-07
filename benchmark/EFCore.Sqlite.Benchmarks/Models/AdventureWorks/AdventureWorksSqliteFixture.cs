// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks
{
    public static class AdventureWorksSqliteFixture
    {
        private static readonly string _baseDirectory
            = Path.GetDirectoryName(new Uri(typeof(AdventureWorksSqliteFixture).Assembly.CodeBase).LocalPath);

        private static readonly string _connectionString
            = $"Data Source={Path.Combine(_baseDirectory, "AdventureWorks2014.db")}";

        // This method is called from timed code, be careful when changing it
        public static AdventureWorksContextBase CreateContext()
        {
            return new AdventureWorksSqliteContext(_connectionString);
        }
    }
}
