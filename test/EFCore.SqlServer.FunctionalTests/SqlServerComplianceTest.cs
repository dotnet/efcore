﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases => new HashSet<Type>
    {
        typeof(ComplexCollectionJsonUpdateTestBase<>) // issue #31252
    };

    protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
}
