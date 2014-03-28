// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations
{
    public class ModelDiffer
    {
        public virtual IReadOnlyList<MigrationOperation> Diff([NotNull] IModel sourceModel, [NotNull] IModel targetModel)
        {
            Check.NotNull(sourceModel, "sourceModel");
            Check.NotNull(targetModel, "targetModel");

            var sourceDatabase = new DatabaseBuilder().Build(sourceModel);
            var targetDatabase = new DatabaseBuilder().Build(targetModel);

            // TODO: Not implemented.

            throw new NotImplementedException();
        }

        // TODO: Rename this method because it is not suggestive of what it does.
        public virtual IReadOnlyList<MigrationOperation> DiffSource([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new DatabaseBuilder().Build(model);

            var createSequenceOperations = database.Sequences.Select(
                s => new CreateSequenceOperation(s));

            var createTableOperations = database.Tables.Select(
                t => new CreateTableOperation(t));

            var addForeignKeyOperations = database.Tables.SelectMany(
                t => t.ForeignKeys,
                (t, fk) => new AddForeignKeyOperation(
                    fk.Name, fk.Table.Name, fk.ReferencedTable.Name,
                    fk.Columns.Select(c => c.Name).ToArray(),
                    fk.ReferencedColumns.Select(c => c.Name).ToArray(),
                    fk.CascadeDelete));

            var createIndexOperations = database.Tables.SelectMany(
                t => t.Indexes,
                (t, idx) => new CreateIndexOperation(
                    idx.Table.Name, idx.Name,
                    idx.Columns.Select(c => c.Name).ToArray(),
                    idx.IsUnique, idx.IsClustered));

            return
                ((IEnumerable<MigrationOperation>)createSequenceOperations)
                    .Union(createTableOperations)
                    .Union(addForeignKeyOperations)
                    .Union(createIndexOperations)
                    .ToArray();
        }

        // TODO: Rename this method because it is not suggestive of what it does.
        public virtual IReadOnlyList<MigrationOperation> DiffTarget([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new DatabaseBuilder().Build(model);

            var dropSequenceOperations = database.Sequences.Select(
                s => new DropSequenceOperation(s.Name));

            var dropForeignKeyOperations = database.Tables.SelectMany(
                t => t.ForeignKeys,
                (t, fk) => new DropForeignKeyOperation(fk.Table.Name, fk.Name));

            var dropTableOperations = database.Tables.Select(
                t => new DropTableOperation(t.Name));

            return
                ((IEnumerable<MigrationOperation>)dropSequenceOperations)
                    .Union(dropForeignKeyOperations)
                    .Union(dropTableOperations)
                    .ToArray();
        }
    }
}
