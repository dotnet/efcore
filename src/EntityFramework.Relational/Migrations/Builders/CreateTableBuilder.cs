// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Builders
{
    public class CreateTableBuilder<TColumns> : OperationBuilder<CreateTableOperation>
    {
        private readonly IReadOnlyDictionary<PropertyInfo, AddColumnOperation> _columnMap;

        public CreateTableBuilder(
            [NotNull] CreateTableOperation operation,
            [NotNull] IReadOnlyDictionary<PropertyInfo, AddColumnOperation> columnMap)
            : base(operation)
        {
            Check.NotNull(columnMap, nameof(columnMap));

            _columnMap = columnMap;
        }

        public virtual OperationBuilder<AddForeignKeyOperation> ForeignKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> columns,
            [NotNull] string referencedTable,
            [NotNull] string referencedColumn,
            [CanBeNull] string referencedSchema = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction) =>
                ForeignKey(
                    name,
                    columns,
                    referencedTable,
                    referencedSchema,
                    new[] { referencedColumn },
                    onUpdate,
                    onDelete);

        public virtual OperationBuilder<AddForeignKeyOperation> ForeignKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> columns,
            [NotNull] string referencedTable,
            [CanBeNull] string referencedSchema = null,
            [CanBeNull] string[] referencedColumns = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));
            Check.NotEmpty(referencedTable, nameof(referencedTable));

            var operation = new AddForeignKeyOperation
            {
                Schema = Operation.Schema,
                Table = Operation.Name,
                Name = name,
                Columns = Map(columns),
                ReferencedSchema = referencedSchema,
                ReferencedTable = referencedTable,
                ReferencedColumns = referencedColumns,
                OnUpdate = onUpdate,
                OnDelete = onDelete
            };
            Operation.ForeignKeys.Add(operation);

            return new OperationBuilder<AddForeignKeyOperation>(operation);
        }

        public virtual OperationBuilder<AddPrimaryKeyOperation> PrimaryKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> columns)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));

            var operation = new AddPrimaryKeyOperation
            {
                Schema = Operation.Schema,
                Table = Operation.Name,
                Name = name,
                Columns = Map(columns)
            };
            // TODO: Throw if already set?
            Operation.PrimaryKey = operation;

            return new OperationBuilder<AddPrimaryKeyOperation>(operation);
        }

        public virtual OperationBuilder<AddUniqueConstraintOperation> Unique(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> columns)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));

            var operation = new AddUniqueConstraintOperation
            {
                Schema = Operation.Schema,
                Table = Operation.Name,
                Name = name,
                Columns = Map(columns)
            };
            Operation.UniqueConstraints.Add(operation);

            return new OperationBuilder<AddUniqueConstraintOperation>(operation);
        }

        public new virtual CreateTableBuilder<TColumns> Annotation([NotNull] string name, [NotNull] string value)
            => (CreateTableBuilder<TColumns>)base.Annotation(name, value);

        private string[] Map(LambdaExpression columns) =>
            columns.GetPropertyAccessList().Select(c => _columnMap[c].Name).ToArray();
    }
}
