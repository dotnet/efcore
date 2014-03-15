// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class RenameTableOperation : MigrationOperation
    {
        private readonly Table _table;
        private readonly string _tableName;

        public RenameTableOperation([NotNull] Table table, [NotNull] string tableName)
        {
            Check.NotNull(table, "table");
            Check.NotEmpty(tableName, "tableName");

            _table = table;
            _tableName = tableName;
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public virtual string TableName
        {
            get { return _tableName; }
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
