// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers
{
    [XunitTestCaseDiscoverer("EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers.AdventureWorksDatabaseTestDiscoverer", "EntityFramework.Microbenchmarks.Core")]
    public class AdventureWorksDatabaseBenchmarkAttribute : BenchmarkAttribute
    {
    }
}
