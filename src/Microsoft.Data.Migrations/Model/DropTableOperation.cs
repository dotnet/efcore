// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class DropTableOperation : MigrationOperation
    {
        private readonly Table _table;

        public DropTableOperation([NotNull] Table table)
        {
            _table = table;
        }

        public virtual Table Table
        {
            get { return _table; }
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
