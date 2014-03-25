// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class AddForeignKeyOperation : MigrationOperation
    {
        private readonly string _foreignKeyName;
        private readonly SchemaQualifiedName _tableName;
        private readonly SchemaQualifiedName _referencedTableName; 
        private readonly IReadOnlyList<string> _columnNames;
        private readonly IReadOnlyList<string> _referencedColumnNames;
        private readonly bool _cascadeDelete;

        public AddForeignKeyOperation(
            [NotNull] string foreignKeyName,
            SchemaQualifiedName tableName,
            SchemaQualifiedName referencedTableName,
            [NotNull] IReadOnlyList<string> columnNames,
            [NotNull] IReadOnlyList<string> referencedColumnNames,
            bool cascadeDelete)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(referencedColumnNames, "referencedColumnNames");

            _foreignKeyName = foreignKeyName;
            _tableName = tableName;
            _referencedTableName = referencedTableName;
            _columnNames = columnNames;
            _referencedColumnNames = referencedColumnNames;
            _cascadeDelete = cascadeDelete;
        }

        public virtual string ForeignKeyName
        {
            get { return _foreignKeyName; }
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual SchemaQualifiedName ReferencedTableName
        {
            get { return _referencedTableName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public virtual IReadOnlyList<string> ReferencedColumnNames
        {
            get { return _referencedColumnNames; }
        }

        public virtual bool CascadeDelete
        {
            get { return _cascadeDelete; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
