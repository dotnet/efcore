// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteOperationTransformer
    {
        private readonly IModelDiffer _differ;
        private readonly ISet<string> _tableRebuilds = new HashSet<string>();

        public SqliteOperationTransformer([NotNull] IModelDiffer differ)
        {
            Check.NotNull(differ, nameof(differ));

            _differ = differ;
        }

        protected IList<MigrationOperation> TransformOperation(MigrationOperation operation, IModel model) 
            => new[] { operation };

        private IList<MigrationOperation> TransformOperation(DropColumnOperation operation, IModel model)
            => CreateTableRebuild(operation.Table, model, operation);

        private IList<MigrationOperation> TransformOperation(AlterColumnOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(AddColumnOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(AddForeignKeyOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(AddPrimaryKeyOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(CreateIndexOperation operation, IModel model)
            => FilterRedudantOperation(operation.Table, operation);

        private IList<MigrationOperation> TransformOperation(CreateTableOperation operation, IModel model)
            => FilterRedudantOperation(operation.Name, operation);

        private IList<MigrationOperation> FilterRedudantOperation(string tableName, MigrationOperation operation)
            => _tableRebuilds.Contains(tableName) ? null : new[] { operation };

        private IList<MigrationOperation> CreateTableRebuild(string tableName, IModel model, MigrationOperation operation)
        {
            if (_tableRebuilds.Contains(tableName))
            {
                return null;
            }

            // TODO ensure this temporary table does not conflict with an existing table
            var tempTableName = tableName + "_temp";

            // TODO find more efficient way to get a create table operation. Expose ModelDiffer.Add?
            var differences = _differ.GetDifferences(null, model);
            var createTableOperation = differences.FirstOrDefault(o => (o as CreateTableOperation)?.Name == tableName);

            if (createTableOperation == null)
            {
                return new[] { operation };
            }

            _tableRebuilds.Add(tableName);

            var rebuildOperations = new List<MigrationOperation>
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
                    Columns = ((CreateTableOperation)createTableOperation)
                        .Columns
                        .Select(c => c.Name)
                        .ToArray()
                },
                new DropTableOperation
                {
                    Name = tempTableName
                }
            };

            rebuildOperations.AddRange(differences.Where(o => (o as CreateIndexOperation)?.Table == tableName));

            return rebuildOperations;
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
            return finalOperations.AsReadOnly();
        }
    }
}
