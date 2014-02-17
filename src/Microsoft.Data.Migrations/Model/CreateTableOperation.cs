// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateTableOperation : MigrationOperation<Table, DropTableOperation>
    {
        public CreateTableOperation([NotNull] Table table)
            : base(Check.NotNull(table, "table"))
        {
        }

        public override DropTableOperation Inverse
        {
            get { return new DropTableOperation(Target); }
        }
    }
}
