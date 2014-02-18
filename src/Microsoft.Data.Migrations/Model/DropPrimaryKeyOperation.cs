// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class DropPrimaryKeyOperation : MigrationOperation<PrimaryKey, AddPrimaryKeyOperation>
    {
        private readonly Table _table;

        public DropPrimaryKeyOperation([NotNull] PrimaryKey primaryKey, [NotNull] Table table)
            : base(Check.NotNull(primaryKey, "primaryKey"))
        {
            Check.NotNull(table, "table");

            _table = table;
        }

        public virtual Table Table
        {
            get { return _table; }
        }

        public override AddPrimaryKeyOperation Inverse
        {
            get { return new AddPrimaryKeyOperation(Target, Table); }
        }
    }
}
