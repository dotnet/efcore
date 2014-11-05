// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Sequence = Microsoft.Data.Entity.Relational.Metadata.Sequence;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerModelDiffer : ModelDiffer
    {
        public SqlServerModelDiffer([NotNull] SqlServerDatabaseBuilder databaseBuilder)
            : base(databaseBuilder)
        {
        }

        public virtual new SqlServerDatabaseBuilder DatabaseBuilder
        {
            get { return (SqlServerDatabaseBuilder)base.DatabaseBuilder; }
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
                new SqlServerMigrationOperationPreProcessor(DatabaseBuilder.TypeMapper)
                    .Process(operations, sourceDatabase, targetDatabase);
        }

        protected override string GetSequenceName(Column column)
        {
            Check.NotNull(column, "column");

            // TODO: This can't use the normal APIs because all the annotations have been
            // copied from the core metadata into the relational model.

            var strategy = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration];

            if (!column.GenerateValueOnAdd
                || strategy != SqlServerValueGenerationStrategy.Sequence.ToString())
            {
                return null;
            }

            var name = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceName]
                       ?? column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName];
            var schema = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceSchema]
                         ?? column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema];

            return name == null
                ? Sequence.DefaultName
                : (schema != null ? schema + "." : "") + name;
        }
    }
}
