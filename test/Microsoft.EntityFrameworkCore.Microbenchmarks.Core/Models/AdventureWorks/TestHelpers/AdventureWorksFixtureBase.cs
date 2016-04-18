// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers
{
    public class AdventureWorksFixtureBase
    {
        public static string ConnectionString { get; } = $@"{BenchmarkConfig.Instance.BenchmarkDatabase}Database=AdventureWorks2014;";
    }
}
