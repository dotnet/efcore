// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class DropColumnOperation : MigrationOperation
    {
        private readonly Column _column;

        public DropColumnOperation([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            _column = column;
        }

        public virtual Column Column
        {
            get { return _column; }
        }

        public override bool IsDestructiveChange
        {
            get { return true; }
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
