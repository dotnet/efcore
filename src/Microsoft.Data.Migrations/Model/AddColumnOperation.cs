// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class AddColumnOperation : MigrationOperation
    {
        private readonly Column _column;
        private readonly Table _table;

        public AddColumnOperation([NotNull] Column column, [NotNull] Table table)
        {
            Check.NotNull(column, "column");
            Check.NotNull(table, "table");

            _column = column;
            _table = table;
        }

        public virtual Column Column
        {
            get { return _column; }
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public override void GenerateOperationSql(
            [NotNull] MigrationOperationSqlGenerator migrationOperationSqlGenerator,
            [NotNull] IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(migrationOperationSqlGenerator, "migrationOperationSqlGenerator");
            Check.NotNull(stringBuilder, "stringBuilder");

            migrationOperationSqlGenerator.Generate(this, stringBuilder, generateIdempotentSql);
        }
    }
}
