// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class TableBuilder<TColumns>
    {
        private readonly CreateTableOperation _createTableOperation;
        private readonly IDictionary<PropertyInfo, ColumnModel> _propertyInfoToColumnMap;

        public TableBuilder(
            [NotNull] CreateTableOperation createTableOperation,
            [NotNull] IDictionary<PropertyInfo, ColumnModel> propertyInfoToColumnMap)
        {
            Check.NotNull(createTableOperation, nameof(createTableOperation));
            Check.NotNull(propertyInfoToColumnMap, nameof(propertyInfoToColumnMap));

            _createTableOperation = createTableOperation;
            _propertyInfoToColumnMap = propertyInfoToColumnMap;
        }

        public virtual TableBuilder<TColumns> PrimaryKey(
            [NotNull] Expression<Func<TColumns, object>> keyExpression,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(keyExpression, nameof(keyExpression));

            var columns = keyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.PrimaryKey = new AddPrimaryKeyOperation(
                _createTableOperation.Name,
                _createTableOperation.Schema,
                name,
                columns,
                annotations);

            return this;
        }

        public virtual TableBuilder<TColumns> UniqueConstraint(
            [NotNull] Expression<Func<TColumns, object>> keyExpression,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(keyExpression, nameof(keyExpression));

            var columns = keyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.UniqueConstraints.Add(
                new AddUniqueConstraintOperation(
                    _createTableOperation.Name,
                    _createTableOperation.Schema,
                    name,
                    columns,
                    annotations));

            return this;
        }

        public virtual TableBuilder<TColumns> ForeignKey(
            [NotNull] Expression<Func<TColumns, object>> dependentKeyExpression,
            [NotNull] string principalTable,
            [NotNull] string principalColumn,
            [CanBeNull] string principalSchema = null,
            bool cascadeDelete = false,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null) =>
            ForeignKey(
                dependentKeyExpression,
                principalTable,
                new[] { principalColumn },
                principalSchema,
                cascadeDelete,
                name,
                annotations);

        public virtual TableBuilder<TColumns> ForeignKey(
            [NotNull] Expression<Func<TColumns, object>> dependentKeyExpression,
            [NotNull] string principalTable,
            [CanBeNull] IReadOnlyList<string> principalColumns = null,
            [CanBeNull] string principalSchema = null,
            bool cascadeDelete = false,
            [CanBeNull] string name = null,
            [CanBeNull] IReadOnlyDictionary<string, string> annotations = null)
        {
            Check.NotNull(dependentKeyExpression, nameof(dependentKeyExpression));
            Check.NotEmpty(principalTable, nameof(principalTable));

            var dependentColumns = dependentKeyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToList();

            _createTableOperation.ForeignKeys.Add(
                new AddForeignKeyOperation(
                    _createTableOperation.Name,
                    _createTableOperation.Schema,
                    name,
                    dependentColumns,
                    principalTable,
                    principalSchema,
                    principalColumns,
                    cascadeDelete,
                    annotations));

            return this;
        }
    }
}
