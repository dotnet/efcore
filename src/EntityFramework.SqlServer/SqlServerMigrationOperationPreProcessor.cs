// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationPreProcessor : MigrationOperationVisitor<SqlServerMigrationOperationPreProcessor.Context>
    {
        private readonly SqlServerTypeMapper _typeMapper;

        public SqlServerMigrationOperationPreProcessor([NotNull] SqlServerTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, "typeMapper");

            _typeMapper = typeMapper;
        }

        public virtual SqlServerTypeMapper TypeMapper
        {
            get { return _typeMapper; }
        }

        public virtual IEnumerable<MigrationOperation> Process(            
            [NotNull] IEnumerable<MigrationOperation> operations,
            [NotNull] DatabaseModel sourceDatabase,
            [NotNull] DatabaseModel targetDatabase)
        {
            Check.NotNull(operations, "operations");
            Check.NotNull(sourceDatabase, "sourceDatabase");
            Check.NotNull(targetDatabase, "targetDatabase");

            var context = new Context(sourceDatabase, targetDatabase);

            foreach (var operation in operations)
            {
                operation.Accept(this, context);
            }

            return context.Operations;
        }

        public override void Visit(DropTableOperation dropTableOperation, Context context)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(context, "context");

            var database = context.SourceDatabase;
            var table = database.GetTable(dropTableOperation.TableName);

            foreach (var foreignKey in database.Tables
                    .SelectMany(t => t.ForeignKeys)
                    .Where(fk => ReferenceEquals(fk.ReferencedTable, table)))
            {
                context.HandleOperation(new DropForeignKeyOperation(foreignKey.Table.Name, foreignKey.Name));
            }

            context.HandleOperation(dropTableOperation);
        }

        public override void Visit(DropColumnOperation dropColumnOperation, Context context)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(context, "context");

            var database = context.SourceDatabase;
            var table = database.GetTable(dropColumnOperation.TableName);
            var column = table.GetColumn(dropColumnOperation.ColumnName);

            if (column.HasDefault)
            {
                context.HandleOperation(new DropDefaultConstraintOperation(table.Name, column.Name));
            }

            context.HandleOperation(dropColumnOperation);
        }

        public override void Visit(AlterColumnOperation alterColumnOperation, Context context)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(context, "context");

            var database = context.SourceDatabase;
            var table = database.GetTable(alterColumnOperation.TableName);
            var column = table.GetColumn(alterColumnOperation.NewColumn.Name);
            var newColumn = alterColumnOperation.NewColumn;

            string dataType, newDataType;
            GetDataTypes(table, column, newColumn, context, out dataType, out newDataType);

            var primaryKey = table.PrimaryKey;
            if (primaryKey != null
                && primaryKey.Columns.Any(c => ReferenceEquals(c, column)))
            {
                if (context.HandleOperation(new DropPrimaryKeyOperation(primaryKey.Table.Name, primaryKey.Name),
                    (x, y) => x.TableName == y.TableName && x.PrimaryKeyName == y.PrimaryKeyName))
                {
                    context.HandleOperation(new AddPrimaryKeyOperation(primaryKey));
                }
            }

            // TODO: Changing the length of a variable-length column used in a UNIQUE constraint is allowed.
            foreach (var uniqueConstraint in table.UniqueConstraints
                .Where(uc => uc.Columns.Any(c => ReferenceEquals(c, column))))
            {
                if (context.HandleOperation(new DropUniqueConstraintOperation(uniqueConstraint.Table.Name, uniqueConstraint.Name),
                    (x, y) => x.TableName == y.TableName && x.UniqueConstraintName == y.UniqueConstraintName))
                {
                    context.HandleOperation(new AddUniqueConstraintOperation(uniqueConstraint));
                }
            }

            foreach (var foreignKey in table.ForeignKeys
                .Where(fk => fk.Columns.Any(c => ReferenceEquals(c, column)))
                .Concat(database.Tables
                    .SelectMany(t => t.ForeignKeys)
                    .Where(fk => fk.ReferencedColumns.Any(c => ReferenceEquals(c, column)))))
            {
                if (context.HandleOperation(new DropForeignKeyOperation(foreignKey.Table.Name, foreignKey.Name),
                    (x, y) => x.TableName == y.TableName && x.ForeignKeyName == y.ForeignKeyName))
                {
                    context.HandleOperation(new AddForeignKeyOperation(foreignKey));
                }
            }

            if (dataType != newDataType
                || ((string.Equals(dataType, "varchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "nvarchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "varbinary", StringComparison.OrdinalIgnoreCase))
                    && newColumn.MaxLength > column.MaxLength))
            {
                foreach (var index in table.Indexes
                    .Where(ix => ix.Columns.Any(c => ReferenceEquals(c, column))))
                {
                    if (context.HandleOperation(new DropIndexOperation(index.Table.Name, index.Name),
                        (x, y) => x.TableName == y.TableName && x.IndexName == y.IndexName))
                    {
                        context.HandleOperation(new CreateIndexOperation(index));
                    }
                }
            }

            if (column.HasDefault)
            {
                context.HandleOperation(new DropDefaultConstraintOperation(table.Name, column.Name));
            }

            if (column.IsTimestamp)
            {
                context.HandleOperation(new DropColumnOperation(table.Name, column.Name));
                context.HandleOperation(new AddColumnOperation(table.Name, newColumn));
            }
            else
            {
                context.HandleOperation(alterColumnOperation);
            }
        }

        protected override void VisitDefault(MigrationOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleOperation(operation);
        }

        protected virtual void GetDataTypes(
            [NotNull] Table table, [NotNull] Column column, [NotNull] Column newColumn, [NotNull] Context context,
            out string dataType, out string newDataType)
        {
            Check.NotNull(table, "table");
            Check.NotNull(column, "column");
            Check.NotNull(newColumn, "newColumn");
            Check.NotNull(context, "context");

            var isKey
                = table.PrimaryKey != null
                  && table.PrimaryKey.Columns.Contains(column)
                  || table.UniqueConstraints.SelectMany(k => k.Columns).Contains(column)
                  || table.ForeignKeys.SelectMany(k => k.Columns).Contains(column);

            dataType
                = TypeMapper.GetTypeMapping(
                    column.DataType, column.Name, column.ClrType, isKey, column.IsTimestamp)
                    .StoreTypeName;
            newDataType
                = TypeMapper.GetTypeMapping(
                    newColumn.DataType, newColumn.Name, newColumn.ClrType, isKey, newColumn.IsTimestamp)
                    .StoreTypeName;
        }

        public class Context
        {
            private readonly DatabaseModel _sourceDatabase;
            private readonly DatabaseModel _targetDatabase;
            private readonly MigrationOperationCollection _operations = new MigrationOperationCollection();

            public Context([NotNull] DatabaseModel sourceDatabase, [NotNull] DatabaseModel targetDatabase)
            {
                Check.NotNull(sourceDatabase, "sourceDatabase");
                Check.NotNull(targetDatabase, "targetDatabase");

                _sourceDatabase = sourceDatabase;
                _targetDatabase = targetDatabase;
            }

            public virtual DatabaseModel SourceDatabase
            {
                get { return _sourceDatabase; }
            }

            public virtual DatabaseModel TargetDatabase
            {
                get { return _targetDatabase; }
            }

            public virtual IReadOnlyList<MigrationOperation> Operations
            {
                get { return _operations.GetAll(); }
            }

            public virtual bool HandleOperation<T>([NotNull] T operation, [CanBeNull] Func<T, T, bool> compareFunc = null)
                where T : MigrationOperation
            {
                Check.NotNull(operation, "operation");

                if (compareFunc != null
                    && _operations.Get<T>().Any(op => compareFunc(op, operation)))
                {
                    return false;
                }

                _operations.Add(operation);

                return true;
            }
        }
    }
}
