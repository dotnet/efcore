// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class DropSequenceOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _sequenceName;

        public DropSequenceOperation(SchemaQualifiedName sequenceName)
        {
            _sequenceName = sequenceName;
        }

        public virtual SchemaQualifiedName SequenceName
        {
            get { return _sequenceName; }
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
