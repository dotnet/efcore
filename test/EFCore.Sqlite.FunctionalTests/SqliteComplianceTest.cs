// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;
using Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class SqliteComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(SqlExecutorTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(TPCRelationshipsQueryTestBase<>), // internal class is added
        typeof(StoredProcedureUpdateTestBase), // SQLite doesn't support stored procedures

        // All tests in the following test suites currently fail because of #26708
        // (Stop generating composite keys for owned collections on SQLite)
        typeof(OwnedNavigationsProjectionTestBase<>),
        typeof(OwnedNavigationsProjectionRelationalTestBase<>),
        typeof(OwnedJsonProjectionRelationalTestBase<>),
        typeof(OwnedTableSplittingProjectionRelationalTestBase<>)
    };

    protected override Assembly TargetAssembly { get; } = typeof(SqliteComplianceTest).Assembly;
}
