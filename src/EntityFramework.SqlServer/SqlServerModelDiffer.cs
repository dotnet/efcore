// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Metadata;
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

            // TODO: This can't use the normal APIs because all the annotations have been
            // copied from the core metadata into the relational model.

            var strategy = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration];

            if (column.ValueGenerationStrategy != ValueGeneration.OnAdd
                || strategy != SqlServerValueGenerationStrategy.Sequence.ToString())
            {
                return null;
            }

            var name = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceName]
                       ?? column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName];
            var schema = column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceSchema]
                         ?? column[SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema];

            return name == null
                ? Relational.Metadata.Sequence.DefaultName
                : (schema != null ? schema + "." : "") + name;
        }
    }
}
