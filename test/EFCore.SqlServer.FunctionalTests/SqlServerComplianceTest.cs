// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases => new List<Type>
        {
            typeof(SaveChangesInterceptionTestBase)
        };

        protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
    }
}
