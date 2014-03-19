// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class AlterColumnOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly Column _column;
        private readonly bool _isDestructiveChange;

        public AlterColumnOperation(SchemaQualifiedName tableName, [NotNull] Column column, bool isDestructiveChange)
        {
            Check.NotNull(column, "column");

            _tableName = tableName;
            _column = column;
            _isDestructiveChange = isDestructiveChange;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual Column Column
        {
            get { return _column; }
        }

        public override bool IsDestructiveChange
        {
            get { return _isDestructiveChange; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
