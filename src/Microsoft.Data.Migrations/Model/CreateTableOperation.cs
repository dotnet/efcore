// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateTableOperation : MigrationOperation
    {
        private readonly Table _table;

        public CreateTableOperation([NotNull] Table table)
        {
            Check.NotNull(table, "table");

            _table = table;
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
