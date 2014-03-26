// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class AlterColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _columnName;
        private readonly Type _clrType;
        private readonly string _dataType;
        private readonly bool _isNullable;
        private readonly bool _isDestructiveChange;

        public AlterColumnOperation(
            SchemaQualifiedName tableName,
            [NotNull] string columnName,
            [CanBeNull] Type clrType,
            [CanBeNull] string dataType,
            bool isNullable,
            bool isDestructiveChange)
        {
            Check.NotNull(columnName, "columnName");

            _tableName = tableName;
            _columnName = columnName;
            _clrType = clrType;
            _dataType = dataType;
            _isNullable = isNullable;
            _isDestructiveChange = isDestructiveChange;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string ColumnName
        {
            get { return _columnName; }
        }

        public virtual Type ClrType
        {
            get { return _clrType; }
        }

        public virtual string DataType
        {
            get { return _dataType; }
        }

        public virtual bool IsNullable
        {
            get { return _isNullable; }
        }

        public override bool IsDestructiveChange
        {
            get { return _isDestructiveChange; }
        }

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator visitor, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(stringBuilder, "stringBuilder");

            visitor.Generate(this, stringBuilder, generateIdempotentSql);
        }
    }
}
