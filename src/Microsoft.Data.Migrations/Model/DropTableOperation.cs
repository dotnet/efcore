// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class DropTableOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;

        public DropTableOperation(SchemaQualifiedName tableName)
        {
            _tableName = tableName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public override bool IsDestructiveChange
        {
            get { return true; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
