// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public class OracleComplianceTest : RelationalComplianceTestBase
    {
        protected override Assembly TargetAssembly { get; } = typeof(OracleComplianceTest).Assembly;
    }
}
