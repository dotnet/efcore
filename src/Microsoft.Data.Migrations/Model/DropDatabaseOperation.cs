// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;

namespace Microsoft.Data.Migrations.Model
{
    public class DropDatabaseOperation : MigrationOperation
    {
        private readonly string _databaseName;

        public DropDatabaseOperation([NotNull] string databaseName)
        {
            Check.NotEmpty(databaseName, "databaseName");

            _databaseName = databaseName;
        }

        public virtual string DatabaseName
        {
            get { return _databaseName; }
        }

        public override bool IsDestructiveChange
        {
            get { return true; }
        }

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator visitor, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(stringBuilder, "stringBuilder");

            visitor.Generate(this, stringBuilder, generateIdempotentSql);
        }
    }
}
