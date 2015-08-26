// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers
{
    public class AdventureWorksFixtureBase
    {
        private static readonly string _connectionString = $@"Server={BenchmarkConfig.Instance.BenchmarkDatabaseInstance};Database=AdventureWorks2014;Integrated Security=True;MultipleActiveResultSets=true;";

        public static string ConnectionString => _connectionString;
    }
}
