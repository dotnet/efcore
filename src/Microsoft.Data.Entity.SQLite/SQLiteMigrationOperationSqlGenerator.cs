// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public SQLiteMigrationOperationSqlGenerator([NotNull] SQLiteTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        protected override void GeneratePrimaryKey(
            string primaryKeyName,
            IReadOnlyList<string> columnNames,
            bool isClustered,
            IndentedStringBuilder stringBuilder)
        {
            // NOTE: SQLite doesn't understand NONCLUSTERED
            base.GeneratePrimaryKey(primaryKeyName, columnNames, true, stringBuilder);
        }
    }
}
