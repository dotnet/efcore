// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        public override string GenerateDataType(Column column)
        {
            Check.NotNull(column, "column");

            if (column.DataType != null)
            {
                return column.DataType;
            }

            var types = SQLiteTypeMap.FromClrType(Nullable.GetUnderlyingType(column.ClrType) ?? column.ClrType)
                .DeclaredTypes;
            Contract.Assert(types.Any(), "types is empty.");

            return types.First();
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
