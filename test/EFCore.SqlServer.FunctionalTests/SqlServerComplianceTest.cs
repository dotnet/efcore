// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerComplianceTest : RelationalComplianceTestBase
    {
        protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
    }
}
