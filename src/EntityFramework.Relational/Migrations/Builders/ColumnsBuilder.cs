// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Builders
{
    public class ColumnsBuilder
    {
        private readonly CreateTableOperation _createTableOperation;

        public ColumnsBuilder([NotNull] CreateTableOperation createTableOperation)
        {
            Check.NotNull(createTableOperation, nameof(createTableOperation));

            _createTableOperation = createTableOperation;
        }

        public virtual OperationBuilder<AddColumnOperation> Column(
            [NotNull] string type,
            [CanBeNull] string name = null,
            bool nullable = false,
            [CanBeNull] object defaultValue = null,
            [CanBeNull] string defaultExpression = null)
        {
            Check.NotEmpty(type, nameof(type));

            var operation = new AddColumnOperation
            {
                Schema = _createTableOperation.Schema,
                Table = _createTableOperation.Name,
                Name = name,
                Type = type,
                IsNullable = nullable,
                DefaultValue = defaultValue,
                DefaultValueSql = defaultExpression
            };
            _createTableOperation.Columns.Add(operation);

            return new OperationBuilder<AddColumnOperation>(operation);
        }
    }
}
