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
        private readonly SchemaQualifiedName _principalTableName; 
        private readonly SchemaQualifiedName _dependentTableName;
        private readonly IReadOnlyList<string> _principalColumnNames;
        private readonly IReadOnlyList<string> _dependentColumnNames;
        private readonly bool _cascadeDelete;

        public AddForeignKeyOperation(
            [NotNull] string foreignKeyName,
            SchemaQualifiedName principalTableName, 
            SchemaQualifiedName dependentTableName,
            [NotNull] IReadOnlyList<string> principalColumnNames,
            [NotNull] IReadOnlyList<string> dependentColumnNames,
            bool cascadeDelete)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");
            Check.NotNull(principalColumnNames, "principalColumnNames");
            Check.NotNull(dependentColumnNames, "dependentColumnNames");

            _foreignKeyName = foreignKeyName;
            _principalTableName = principalTableName;
            _dependentTableName = dependentTableName;
            _principalColumnNames = principalColumnNames;
            _dependentColumnNames = dependentColumnNames;
            _cascadeDelete = cascadeDelete;
        }

        public virtual string ForeignKeyName
        {
            get { return _foreignKeyName; }
        }

        public virtual SchemaQualifiedName PrincipalTableName
        {
            get { return _principalTableName; }
        }

        public virtual SchemaQualifiedName DependentTableName
        {
            get { return _dependentTableName; }
        }

        public virtual IReadOnlyList<string> PrincipalColumnNames
        {
            get { return _principalColumnNames; }
        }

        public virtual IReadOnlyList<string> DependentColumnNames
        {
            get { return _dependentColumnNames; }
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
