// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModelDiffer : ModelDiffer
    {
        public SqlServerModelDiffer([NotNull] SqlServerDatabaseBuilder databaseBuilder)
            : base(databaseBuilder)
        {
        }

        protected override string GetSequenceName(Column column)
        {
            Check.NotNull(column, "column");

            return column.GetSequenceName();
        }
    }
}
