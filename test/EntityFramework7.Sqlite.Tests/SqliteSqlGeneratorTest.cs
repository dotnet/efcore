// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator() => new SqliteUpdateSqlGenerator();
        protected override string RowsAffected => "changes()";
        protected override string Identity => "last_insert_rowid()";
        protected override string SchemaName => null;
    }
}
