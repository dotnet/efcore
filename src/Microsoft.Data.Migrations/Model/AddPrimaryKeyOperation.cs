// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class AddPrimaryKeyOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _primaryKeyName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly bool _isClustered;

        public AddPrimaryKeyOperation(
            SchemaQualifiedName tableName,
            [NotNull] string primaryKeyName,
            [NotNull] IReadOnlyList<string> columnNames,
            bool isClustered)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");
            Check.NotNull(columnNames, "columnNames");

            _tableName = tableName;
            _primaryKeyName = primaryKeyName;
            _columnNames = columnNames;
            _isClustered = isClustered;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string PrimaryKeyName
        {
            get { return _primaryKeyName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public virtual bool IsClustered
        {
            get { return _isClustered; }
        }

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator visitor, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(stringBuilder, "stringBuilder");

            visitor.Generate(this, stringBuilder, generateIdempotentSql);
        }
    }
}
