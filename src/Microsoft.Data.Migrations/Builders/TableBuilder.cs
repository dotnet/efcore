// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Builders
{
    public class TableBuilder<TColumns>
    {
        private readonly CreateTableOperation _createTableOperation;
        private readonly MigrationBuilder _migrationBuilder;

        public TableBuilder(
            [NotNull] CreateTableOperation createTableOperation,
            [NotNull] MigrationBuilder migrationBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(migrationBuilder, "migrationBuilder");

            _createTableOperation = createTableOperation;
            _migrationBuilder = migrationBuilder;
        }

        public virtual TableBuilder<TColumns> PrimaryKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> primaryKeyExpression,
            bool clustered = true)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(primaryKeyExpression, "primaryKeyExpression");

            var table = _createTableOperation.Table;
            var columns = primaryKeyExpression.GetPropertyAccessList()
                .Select(p => table.Columns.Single(c => c.ApiPropertyInfo == p))
                .ToArray();

            _createTableOperation.Table.PrimaryKey = new PrimaryKey(name, columns, clustered);

            return this;
        }

        public virtual TableBuilder<TColumns> ForeignKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> foreignKeyExpression,
            [NotNull] string referencedTableName,
            [NotNull] IReadOnlyList<string> referencedColumnNames,
            bool cascadeDelete = false)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(foreignKeyExpression, "foreignKeyExpression");
            Check.NotEmpty(referencedTableName, "referencedTableName");
            Check.NotNull(referencedColumnNames, "referencedColumnNames");

            var table = _createTableOperation.Table;
            var columnNames = foreignKeyExpression.GetPropertyAccessList()
                .Select(p => table.Columns.Single(c => c.ApiPropertyInfo == p).Name)
                .ToArray();
            var addForeignKeyOperation = new AddForeignKeyOperation(table.Name, name,
                columnNames, referencedTableName, referencedColumnNames, cascadeDelete);

            _migrationBuilder.AddOperation(addForeignKeyOperation);

            return this;
        }

        public virtual TableBuilder<TColumns> Index(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> indexExpression,
            bool unique = false,
            bool clustered = false)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(indexExpression, "indexExpression");

            var table = _createTableOperation.Table;
            var columnNames = indexExpression.GetPropertyAccessList()
                .Select(p => table.Columns.Single(c => c.ApiPropertyInfo == p).Name)
                .ToArray();
            var createIndexOperation = new CreateIndexOperation(table.Name, name,
                columnNames, unique, clustered);

            _migrationBuilder.AddOperation(createIndexOperation);

            return this;
        }
    }
}
