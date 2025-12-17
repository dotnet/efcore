// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates.Inheritance;
using Microsoft.EntityFrameworkCore.Query.Inheritance;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqlServerComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } =
    [
        // TODO: #35025
        typeof(TPHInheritanceJsonQueryRelationalTestBase<>),
        typeof(TPTFiltersInheritanceQueryTestBase<>),
        typeof(TPTInheritanceJsonQueryRelationalTestBase<>),
        typeof(TPTInheritanceQueryTestBase<>),
        typeof(TPTInheritanceTableSplittingQueryRelationalTestBase<>),
        typeof(TPTFiltersInheritanceBulkUpdatesTestBase<>),
        typeof(TPTInheritanceBulkUpdatesTestBase<>)
    ];

    protected override Assembly TargetAssembly { get; } = typeof(SqlServerComplianceTest).Assembly;
}
