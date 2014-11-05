// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteModelDiffer : ModelDiffer
    {
        public SQLiteModelDiffer([NotNull] SQLiteDatabaseBuilder databaseBuilder)
            : base(databaseBuilder)
        {
        }

        public virtual new SQLiteDatabaseBuilder DatabaseBuilder
        {
            get { return (SQLiteDatabaseBuilder)base.DatabaseBuilder; }
        }

        protected override IReadOnlyList<MigrationOperation> Process(
            MigrationOperationCollection operations,
            DatabaseModel sourceDatabase,
            DatabaseModel targetDatabase)
        {
            Check.NotNull(operations, "operations");
            Check.NotNull(sourceDatabase, "sourceDatabase");
            Check.NotNull(targetDatabase, "targetDatabase");

            return
                new SQLiteMigrationOperationPreProcessor(DatabaseBuilder.TypeMapper)
                    .Process(operations, sourceDatabase, targetDatabase);
        }

        protected override string GetSequenceName(Column column)
        {
            return null;
        }
    }
}
