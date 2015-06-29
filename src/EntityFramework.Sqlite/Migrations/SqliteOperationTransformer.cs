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

        public SqliteOperationTransformer([NotNull] IModelDiffer differ)
        {
            Check.NotNull(differ, nameof(differ));

            _differ = differ;
        }

        protected IList<MigrationOperation> TransformOperation(MigrationOperation operation, IModel model) =>
            new[] { operation };

        // TODO prevent multiple DropColumnOperations on the same table
        private IList<MigrationOperation> TransformOperation(DropColumnOperation operation, IModel model)
        {
            Check.NotNull(model, nameof(model));

            // TODO ensure this temporary table does not conflict with an existing table
            var tempTableName = operation.Table + "_temp";

            // TODO find more efficient way to get a create table operation. Expose ModelDiffer.Add?
            var createTableOperation = _differ.GetDifferences(null, model)
                .FirstOrDefault(o => (o as CreateTableOperation)?.Name == operation.Table);

            if (!(createTableOperation is CreateTableOperation))
            {
                return new MigrationOperation[] { operation };
            }

            return new[]
            {
                new RenameTableOperation
                {
                    Name = operation.Table,
                    NewName = tempTableName
                },
                createTableOperation,
                new MoveDataOperation
                {
                    OldTable = tempTableName,
                    NewTable = operation.Table,
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
        }

        public virtual IReadOnlyList<MigrationOperation> Transform(
            [NotNull] IReadOnlyList<MigrationOperation> operations,
            [CanBeNull] IModel model = null)
        {
            Check.NotNull(operations, nameof(operations));

            var finalOperations = new List<MigrationOperation>();
            foreach (var operation in operations)
            {
                finalOperations.AddRange(TransformOperation((dynamic)operation, model));
            }
            return finalOperations.AsReadOnly();
        }
    }
}
