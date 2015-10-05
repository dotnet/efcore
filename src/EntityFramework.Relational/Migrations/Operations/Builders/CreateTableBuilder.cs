// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Operations.Builders
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
            [NotNull] Expression<Func<TColumns, object>> column,
            [NotNull] string principalTable,
            [NotNull] string principalColumn,
            [CanBeNull] string principalSchema = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
            => ForeignKey(
                name,
                column,
                principalTable,
                new[] { principalColumn },
                principalSchema,
                onUpdate,
                onDelete);

        public virtual OperationBuilder<AddForeignKeyOperation> ForeignKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> columns,
            [NotNull] string principalTable,
            [NotNull] string[] principalColumns,
            [CanBeNull] string principalSchema = null,
            ReferentialAction onUpdate = ReferentialAction.NoAction,
            ReferentialAction onDelete = ReferentialAction.NoAction)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(columns, nameof(columns));
            Check.NotEmpty(principalTable, nameof(principalTable));

            var operation = new AddForeignKeyOperation
            {
                Schema = Operation.Schema,
                Table = Operation.Name,
                Name = name,
                Columns = Map(columns),
                PrincipalSchema = principalSchema,
                PrincipalTable = principalTable,
                PrincipalColumns = principalColumns,
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

        public virtual OperationBuilder<AddUniqueConstraintOperation> UniqueConstraint(
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

        public new virtual CreateTableBuilder<TColumns> HasAnnotation([NotNull] string name, [NotNull] object value)
            => (CreateTableBuilder<TColumns>)base.HasAnnotation(name, value);

        private string[] Map(LambdaExpression columns)
            => columns.GetPropertyAccessList().Select(c => _columnMap[c].Name).ToArray();
    }
}
