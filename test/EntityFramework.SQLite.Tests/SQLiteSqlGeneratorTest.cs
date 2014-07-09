// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Tests;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteSqlGeneratorTest : SqlGeneratorTestBase
    {
        protected override SqlGenerator CreateSqlGenerator()
        {
            return new SQLiteSqlGenerator();
        }

        protected override string RowsAffected
        {
            get { return "changes()"; }
        }

        protected override string Identity
        {
            get { return "last_insert_rowid()"; }
        }
    }
}
