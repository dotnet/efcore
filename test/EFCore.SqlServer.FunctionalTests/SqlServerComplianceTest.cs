// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore;

public class SqlServerComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } =
    [
        // SQL Server doesn't support ExecuteMerge yet
        typeof(ExecuteMergeTestBase<>),
        typeof(ExecuteMergeRelationalTestBase<>)
    ];

    protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
}
