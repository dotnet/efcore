// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class MoveTableOperation : MigrationOperation
    {
        private readonly Table _table;
        private readonly string _schema;

        public MoveTableOperation([NotNull] Table table, [NotNull] string schema)
        {
            Check.NotEmpty(schema, "schema");

            _table = table;
            _schema = schema;
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public virtual string Schema
        {
            get { return _schema; }
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
