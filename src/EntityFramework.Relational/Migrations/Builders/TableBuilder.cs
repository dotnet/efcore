// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations.Builders
{
    public class TableBuilder<TColumns>
    {
        private readonly CreateTableOperation _createTableOperation;
        private readonly IDictionary<PropertyInfo, Column> _propertyInfoToColumnMap;

        public TableBuilder(
            [NotNull] CreateTableOperation createTableOperation,
            [NotNull] IDictionary<PropertyInfo, Column> propertyInfoToColumnMap)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(propertyInfoToColumnMap, "propertyInfoToColumnMap");

            _createTableOperation = createTableOperation;
            _propertyInfoToColumnMap = propertyInfoToColumnMap;
        }

        public virtual TableBuilder<TColumns> PrimaryKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> primaryKeyExpression,
            bool clustered = true)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(primaryKeyExpression, "primaryKeyExpression");

            var columnNames = primaryKeyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.PrimaryKey = new AddPrimaryKeyOperation(
                _createTableOperation.TableName, name, columnNames, clustered);

            return this;
        }

        public virtual TableBuilder<TColumns> UniqueConstraint(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> uniqueConstraintExpression)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(uniqueConstraintExpression, "uniqueConstraintExpression");

            var columnNames = uniqueConstraintExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.UniqueConstraints.Add(new AddUniqueConstraintOperation(
                _createTableOperation.TableName, name, columnNames));

            return this;
        }

        public virtual TableBuilder<TColumns> ForeignKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> foreignKeyExpression,
            [NotNull] string referencedTableName,
            [NotNull] IReadOnlyList<string> referencedColumnNames,
            bool cascadeDelete = false)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(foreignKeyExpression, "foreignKeyExpression");
            Check.NotEmpty(referencedTableName, "referencedTableName");
            Check.NotNull(referencedColumnNames, "referencedColumnNames");

            var columnNames = foreignKeyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.ForeignKeys.Add(new AddForeignKeyOperation(
                _createTableOperation.TableName, name, columnNames, referencedTableName, referencedColumnNames, cascadeDelete));

            return this;
        }

        public virtual TableBuilder<TColumns> Index(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> indexExpression,
            bool unique = false,
            bool clustered = false)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(indexExpression, "indexExpression");

            var columnNames = indexExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();

            _createTableOperation.Indexes.Add(new CreateIndexOperation(
                _createTableOperation.TableName, name, columnNames, unique, clustered));

            return this;
        }
    }
}
