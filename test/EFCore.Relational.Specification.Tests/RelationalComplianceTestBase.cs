// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class RelationalComplianceTestBase : ComplianceTestBase
{
    protected override IEnumerable<Type> GetBaseTestClasses()
        => base.GetBaseTestClasses().Concat(
            typeof(RelationalComplianceTestBase).Assembly.ExportedTypes.Where(t => t.Name.Contains("TestBase")));
}
