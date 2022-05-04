// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class SqliteComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(SqlExecutorTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(TPCFiltersInheritanceQueryTestBase<>),
        typeof(TPCGearsOfWarQueryRelationalTestBase<>),
        typeof(TPCInheritanceQueryTestBase<>),
        typeof(TPCManyToManyNoTrackingQueryRelationalTestBase<>),
        typeof(TPCManyToManyQueryRelationalTestBase<>),
        typeof(TPCRelationshipsQueryTestBase<>),
    };

    protected override Assembly TargetAssembly { get; } = typeof(SqliteComplianceTest).Assembly;
}
