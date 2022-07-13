// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public class InitializationSqliteTests : InitializationTests
    {
        protected override AdventureWorksContextBase CreateContext()=> AdventureWorksSqliteFixture.CreateContext();
        protected override ConventionSet CreateConventionSet() => SqliteConventionSetBuilder.Build();
    }
}
