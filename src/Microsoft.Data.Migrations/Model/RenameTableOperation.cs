// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class RenameTableOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _newTableName;

        public RenameTableOperation(SchemaQualifiedName tableName, [NotNull] string newTableName)
        {
            Check.NotEmpty(tableName, "tableName");

            _tableName = tableName;
            _newTableName = newTableName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string NewTableName
        {
            get { return _newTableName; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
