// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class RenameColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _columnName;
        private readonly string _newColumnName;

        public RenameColumnOperation(SchemaQualifiedName tableName, [NotNull] string columnName, [NotNull] string newColumnName)
        {
            Check.NotEmpty(columnName, "columnName");
            Check.NotEmpty(newColumnName, "newColumnName");

            _tableName = tableName;
            _columnName = columnName;
            _newColumnName = newColumnName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string ColumnName
        {
            get { return _columnName; }
        }

        public virtual string NewColumnName
        {
            get { return _newColumnName; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
