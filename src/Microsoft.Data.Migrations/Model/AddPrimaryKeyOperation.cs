// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class AddPrimaryKeyOperation : MigrationOperation
    {
        private readonly PrimaryKey _primaryKey;
        private readonly Table _table;

        public AddPrimaryKeyOperation([NotNull] PrimaryKey primaryKey, [NotNull] Table table)
        {
            Check.NotNull(primaryKey, "primaryKey");
            Check.NotNull(table, "table");

            _primaryKey = primaryKey;
            _table = table;
        }

        public virtual PrimaryKey PrimaryKey
        {
            get { return _primaryKey; }
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
