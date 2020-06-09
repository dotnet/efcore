// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public class InitializationSqlServerTests : InitializationTests
    {
        protected override AdventureWorksContextBase CreateContext() => AdventureWorksSqlServerFixture.CreateContext();
        protected override ConventionSet CreateConventionSet() => SqlServerConventionSetBuilder.Build();
    }
}
