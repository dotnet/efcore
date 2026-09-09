// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        typeof(StoredProcedureUpdateTestBase) // SQLite doesn't support stored procedures
    };

    protected override Assembly TargetAssembly { get; } = typeof(SqliteComplianceTest).Assembly;
}
