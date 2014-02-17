// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class AddPrimaryKeyOperation : MigrationOperation<PrimaryKey, DropPrimaryKeyOperation>
    {
        private readonly Table _table;

        public AddPrimaryKeyOperation([NotNull] PrimaryKey primaryKey, [NotNull] Table table)
            : base(Check.NotNull(primaryKey, "primaryKey"))
        {
            Check.NotNull(table, "table");

            _table = table;
        }

        public override void GenerateOperationSql(
            MigrationOperationSqlGenerator migrationOperationSqlGenerator,
            IndentedStringBuilder stringBuilder,
            bool generateIdempotentSql)
        {
            Check.NotNull(migrationOperationSqlGenerator, "migrationOperationSqlGenerator");
            Check.NotNull(stringBuilder, "stringBuilder");

            migrationOperationSqlGenerator.Generate(this, stringBuilder, generateIdempotentSql);
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public override DropPrimaryKeyOperation Inverse
        {
            get { return new DropPrimaryKeyOperation(Target, Table); }
        }
    }
}
