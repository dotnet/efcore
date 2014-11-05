// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;

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

        public override IReadOnlyList<MigrationOperation> Diff(IModel sourceModel, IModel targetModel)
        {
            return new SQLiteMigrationOperationPreProcessor(DatabaseBuilder.TypeMapper).Process(
                base.Diff(sourceModel, targetModel), SourceMapping.Database, TargetMapping.Database).ToList();
        }

        protected override string GetSequenceName(Column column)
        {
            return null;
        }
    }
}
