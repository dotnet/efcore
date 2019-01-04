// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class RelationalComplianceTestBase : ComplianceTestBase
    {
        protected override IEnumerable<Type> GetBaseTestClasses()
            => base.GetBaseTestClasses().Concat(
                typeof(RelationalComplianceTestBase).Assembly.ExportedTypes.Where(t => t.Name.Contains("TestBase")));
    }
}
