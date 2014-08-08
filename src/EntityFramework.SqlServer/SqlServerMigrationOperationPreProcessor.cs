// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationPreProcessor : MigrationOperationVisitor<SqlServerMigrationOperationPreProcessor.Context>
    {
        public override void Visit(DropColumnOperation dropColumnOperation, Context context)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(context, "context");

            var compositeOperation = new CompositeOperation();

            var database = context.Generator.Database;
            var table = database.GetTable(dropColumnOperation.TableName);
            var column = table.GetColumn(dropColumnOperation.ColumnName);

            if (column.HasDefault)
            {
                compositeOperation.AddOperation(new DropDefaultConstraintOperation(table.Name, column.Name));
            }

            compositeOperation.AddOperation(dropColumnOperation);

            context.ProcessCompositeOperation(compositeOperation);
        }

        public override void Visit(AlterColumnOperation alterColumnOperation, Context context)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(context, "context");

            var compositeOperation
                = context.CompositeOperation as CompositeAlterColumnOperation
                  ?? new CompositeAlterColumnOperation();

            var database = context.Generator.Database;
            var table = database.GetTable(alterColumnOperation.TableName);
            var column = table.GetColumn(alterColumnOperation.NewColumn.Name);
            var newColumn = alterColumnOperation.NewColumn;

            string dataType, newDataType;
            GetDataTypes(table, column, newColumn, context, out dataType, out newDataType);

            if (table.PrimaryKey != null
                && table.PrimaryKey.Columns.Any(c => ReferenceEquals(c, column)))
            {
                compositeOperation.AddPrimaryKey(table.PrimaryKey);
            }

            compositeOperation.AddForeignKeys(
                table.ForeignKeys
                    .Where(fk => fk.Columns.Any(c => ReferenceEquals(c, column))));
            compositeOperation.AddForeignKeys(
                database.Tables
                    .SelectMany(t => t.ForeignKeys)
                    .Where(fk => fk.ReferencedColumns.Any(c => ReferenceEquals(c, column))));

            if (dataType != newDataType
                || ((string.Equals(dataType, "varchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "nvarchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "varbinary", StringComparison.OrdinalIgnoreCase))
                    && newColumn.MaxLength > column.MaxLength))
            {
                compositeOperation.AddIndexes(
                    table.Indexes
                        .Where(ix => ix.Columns.Any(c => ReferenceEquals(c, column))));
            }

            if (column.HasDefault)
            {
                compositeOperation.AddOperation(new DropDefaultConstraintOperation(table.Name, column.Name));
            }

            if (column.IsTimestamp)
            {
                compositeOperation.AddOperation(new DropColumnOperation(table.Name, column.Name));
                compositeOperation.AddOperation(new AddColumnOperation(table.Name, newColumn));
            }
            else
            {
                compositeOperation.AddOperation(alterColumnOperation);   
            }

            context.ProcessCompositeOperation(compositeOperation);
        }

        protected override void VisitDefault(MigrationOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.ProcessSimpleOperation(operation);
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
                  || table.ForeignKeys.SelectMany(k => k.Columns).Contains(column);

            dataType
                = context.Generator.TypeMapper.GetTypeMapping(
                    column.DataType, column.Name, column.ClrType, isKey, column.IsTimestamp)
                    .StoreTypeName;
            newDataType
                = context.Generator.TypeMapper.GetTypeMapping(
                    newColumn.DataType, newColumn.Name, newColumn.ClrType, isKey, newColumn.IsTimestamp)
                    .StoreTypeName;
        }

        public class Context
        {
            private readonly SqlServerMigrationOperationSqlGenerator _generator;            
            private readonly List<SqlStatement> _statements = new List<SqlStatement>();

            public Context([NotNull] SqlServerMigrationOperationSqlGenerator generator)
            {
                Check.NotNull(generator, "generator");

                _generator = generator;
            }

            public virtual SqlServerMigrationOperationSqlGenerator Generator
            {
                get { return _generator; }
            }

            public virtual IReadOnlyList<SqlStatement> Statements
            {
                get
                {
                    ProcessCompositeOperation(null);

                    return _statements;
                }
            }

            protected internal virtual CompositeOperation CompositeOperation { get; [param: CanBeNull] private set; }

            public virtual void ProcessSimpleOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                ProcessCompositeOperation(null);

                _statements.AddRange(_generator.Generate(operation));
            }

            public virtual void ProcessCompositeOperation([CanBeNull] CompositeOperation operation)
            {
                if (ReferenceEquals(operation, CompositeOperation))
                {
                    return;
                }

                if (CompositeOperation != null)
                {
                    _statements.AddRange(CompositeOperation.Operations.SelectMany(op => _generator.Generate(op)));
                }

                CompositeOperation = operation;
            }
        }

        public class CompositeOperation
        {
            private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();

            public virtual IEnumerable<MigrationOperation> Operations 
            { 
                get { return _operations; }
            }

            public virtual void AddOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                _operations.Add(operation);
            }
        }

        public class CompositeAlterColumnOperation : CompositeOperation
        {
            private readonly List<PrimaryKey> _primaryKeys = new List<PrimaryKey>();
            private readonly List<ForeignKey> _foreignKeys = new List<ForeignKey>();
            private readonly List<Index> _indexes = new List<Index>();

            public override IEnumerable<MigrationOperation> Operations
            {
                get
                {
                    return
                        ((IEnumerable<MigrationOperation>)_indexes.Select(ix => new DropIndexOperation(ix.Table.Name, ix.Name)))
                            .Concat(_foreignKeys.Select(fk => new DropForeignKeyOperation(fk.Table.Name, fk.Name)))
                            .Concat(_primaryKeys.Select(pk => new DropPrimaryKeyOperation(pk.Table.Name, pk.Name)))
                            .Concat(base.Operations)
                            .Concat(_primaryKeys.Select(pk => new AddPrimaryKeyOperation(pk)))
                            .Concat(_foreignKeys.Select(fk => new AddForeignKeyOperation(fk)))
                            .Concat(_indexes.Select(ix => new CreateIndexOperation(ix)));
                }
            }

            public virtual void AddPrimaryKey([NotNull] PrimaryKey primaryKey)
            {
                Check.NotNull(primaryKey, "primaryKey");

                if (!_primaryKeys.Contains(primaryKey))
                {
                    _primaryKeys.Add(primaryKey);
                }
            }

            public virtual void AddForeignKeys([NotNull] IEnumerable<ForeignKey> foreignKeys)
            {
                Check.NotNull(foreignKeys, "foreignKeys");

                _foreignKeys.AddRange(foreignKeys.Where(fk => !_foreignKeys.Contains(fk)));
            }

            public virtual void AddIndexes([NotNull] IEnumerable<Index> indexes)
            {
                Check.NotNull(indexes, "indexes");

                _indexes.AddRange(indexes.Where(ix => !_indexes.Contains(ix)));
            }
        }
    }
}
