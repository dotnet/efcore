// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.Migrations.Builders
{
    public class TableBuilder<TColumns>
    {
        private readonly CreateTableOperation _createTableOperation;
        private readonly MigrationBuilder _migrationBuilder;
        private readonly IDictionary<PropertyInfo, Column> _propertyInfoToColumnMap;

        public TableBuilder(
            [NotNull] CreateTableOperation createTableOperation,
            [NotNull] MigrationBuilder migrationBuilder,
            [NotNull] IDictionary<PropertyInfo, Column> propertyInfoToColumnMap)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(migrationBuilder, "migrationBuilder");
            Check.NotNull(propertyInfoToColumnMap, "propertyInfoToColumnMap");

            _createTableOperation = createTableOperation;
            _migrationBuilder = migrationBuilder;
            _propertyInfoToColumnMap = propertyInfoToColumnMap;
        }

        public virtual TableBuilder<TColumns> PrimaryKey(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> primaryKeyExpression,
            bool clustered = true)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(primaryKeyExpression, "primaryKeyExpression");

            var columns = primaryKeyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p])
                .ToArray();

            _createTableOperation.Table.PrimaryKey = new PrimaryKey(name, columns, clustered);

            return this;
        }

        public virtual TableBuilder<TColumns> UniqueConstraint(
            [NotNull] string name,
            [NotNull] Expression<Func<TColumns, object>> uniqueConstraintExpression)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(uniqueConstraintExpression, "uniqueConstraintExpression");

            var columns = uniqueConstraintExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p])
                .ToArray();

            _createTableOperation.Table.AddUniqueConstraint(new UniqueConstraint(name, columns));

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

            var table = _createTableOperation.Table;
            var columnNames = foreignKeyExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();
            var addForeignKeyOperation = new AddForeignKeyOperation(table.Name, name,
                columnNames, referencedTableName, referencedColumnNames, cascadeDelete);

            _migrationBuilder.AddOperation(addForeignKeyOperation);

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

            var table = _createTableOperation.Table;
            var columnNames = indexExpression.GetPropertyAccessList()
                .Select(p => _propertyInfoToColumnMap[p].Name)
                .ToArray();
            var createIndexOperation = new CreateIndexOperation(table.Name, name,
                columnNames, unique, clustered);

            _migrationBuilder.AddOperation(createIndexOperation);

            return this;
        }
    }
}
