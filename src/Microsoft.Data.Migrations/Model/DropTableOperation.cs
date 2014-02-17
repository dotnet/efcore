// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class DropTableOperation : MigrationOperation<Table, CreateTableOperation>
    {
        public DropTableOperation([NotNull] Table table)
            : base(Check.NotNull(table, "table"))
        {
        }

        public override CreateTableOperation Inverse
        {
            get { return new CreateTableOperation(Target); }
        }
    }
}
