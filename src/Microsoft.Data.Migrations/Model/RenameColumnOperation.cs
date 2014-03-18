// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class RenameColumnOperation : MigrationOperation
    {
        private readonly Column _column;
        private readonly string _columnName;

        public RenameColumnOperation([NotNull] Column column, [NotNull] string columnName)
        {
            Check.NotNull(column, "column");
            Check.NotEmpty(columnName, "columnName");

            _column = column;
            _columnName = columnName;
        }

        public virtual Column Column
        {
            get { return _column; }
        }

        public virtual string ColumnName
        {
            get { return _columnName; }
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
