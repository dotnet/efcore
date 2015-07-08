// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteOperationTransformer
    {
        private readonly IModelDiffer _differ;
        private readonly IDictionary<string, TableRebuildOperation> _tableRebuilds = new Dictionary<string, TableRebuildOperation>();

        public SqliteOperationTransformer([NotNull] IModelDiffer differ)
        {
            Check.NotNull(differ, nameof(differ));

            _differ = differ;
        }

        protected IList<MigrationOperation> TransformOperation(MigrationOperation operation, IModel model)
            => new[] { operation };

        private IList<MigrationOperation> TransformOperation(DropColumnOperation operation, IModel model)
        {
            GetOrAddRebuild(operation.Table, model);
            return null;
        }

        private IList<MigrationOperation> TransformOperation(RenameColumnOperation operation, IModel model)
        {
            var rebuild = GetOrAddRebuild(operation.Table, model);
            var move = rebuild.Operations.OfType<MoveDataOperation>().First();
            move.ColumnMapping[operation.NewName] = operation.Name;
            return null;
        }

        private IList<MigrationOperation> TransformOperation(AlterColumnOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(AddColumnOperation operation, IModel model)
        {
            TableRebuildOperation rebuild;
            if (!_tableRebuilds.TryGetValue(operation.Table, out rebuild))
            {
                return new MigrationOperation[] { operation };
            }
            var move = rebuild.Operations.OfType<MoveDataOperation>().First();
            if (move.ColumnMapping.ContainsKey(operation.Name))
            {
                move.ColumnMapping.Remove(operation.Name);
            }

            return null;
        }

        private IList<MigrationOperation> TransformOperation(AddPrimaryKeyOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(CreateIndexOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(CreateTableOperation operation, IModel model)
            => FilterRedudantOperation(operation.Name, operation);

        private IList<MigrationOperation> FilterRedudantOperation(string tableName, MigrationOperation operation)
            => _tableRebuilds.ContainsKey(tableName) ? null : new[] { operation };

        private TableRebuildOperation GetOrAddRebuild(string tableName, IModel model)
        {
            TableRebuildOperation rebuild;
            if (!_tableRebuilds.TryGetValue(tableName, out rebuild))
            {
                rebuild = CreateTableRebuild(tableName, model);
                _tableRebuilds.Add(tableName, rebuild);
            }
            return rebuild;
        }

        private TableRebuildOperation CreateTableRebuild(string tableName, IModel model)
        {
            // TODO ensure this temporary table does not conflict with an existing table
            var tempTableName = tableName + "_temp";

            // TODO find more efficient way to get a create table operation. Expose ModelDiffer.Add?
            var differences = _differ.GetDifferences(null, model);
            var createTableOperation = differences
                .OfType<CreateTableOperation>()
                .FirstOrDefault(o => o.Name == tableName);

            var rebuild = new TableRebuildOperation
            {
                Table = tableName,
                Operations =
                {
                    new RenameTableOperation
                    {
                        Name = tableName,
                        NewName = tempTableName
                    },
                    createTableOperation,
                    new MoveDataOperation
                    {
                        OldTable = tempTableName,
                        NewTable = tableName,
                        ColumnMapping = createTableOperation?
                            .Columns
                            .ToDictionary(c => c.Name, c => c.Name)
                    },
                    new DropTableOperation
                    {
                        Name = tempTableName
                    }
                }
            };

            rebuild.Operations.AddRange(differences
                .OfType<CreateIndexOperation>()
                .Where(o => o.Table == tableName));
            return rebuild;
        }

        public virtual IReadOnlyList<MigrationOperation> Transform(
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [CanBeNull] IModel model = null)
        {
            Check.NotNull(operations, nameof(operations));

            var finalOperations = new List<MigrationOperation>();
            foreach (var operation in operations)
            {
                var newOps = TransformOperation((dynamic)operation, model);
                if (newOps == null)
                {
                    continue;
                }
                finalOperations.AddRange(newOps);
            }

            finalOperations.AddRange(_tableRebuilds.Values);

            return finalOperations.AsReadOnly();
        }
    }
}
