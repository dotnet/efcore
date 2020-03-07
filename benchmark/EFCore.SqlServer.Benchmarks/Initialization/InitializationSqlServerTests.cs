// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public class InitializationSqlServerTests : InitializationTests<ColdStartEnabledSqlServerTest>
    {
        protected override ConventionSet CreateConventionSet()
        {
            return SqlServerConventionSetBuilder.Build();
        }
    }
}
