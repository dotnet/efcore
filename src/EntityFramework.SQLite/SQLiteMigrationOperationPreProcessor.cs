// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationPreProcessor : MigrationOperationVisitor<SQLiteMigrationOperationPreProcessor.Context>
    {
        public override void Visit(CreateTableOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.GetHandler(operation.Table.Name);

            if (handler != null)
            {
                context.HandlePendingOperations();
            }

            context.SetHandler(new CreateTableHandler(operation));
        }

        public override void Visit(RenameTableOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);

            handler.AddOperation(operation);
            handler.TableName = new SchemaQualifiedName(operation.NewTableName, operation.TableName.Schema);
        }

        public override void Visit(MoveTableOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);

            handler.AddOperation(operation);
            handler.TableName = new SchemaQualifiedName(operation.TableName.Name, operation.NewSchema);
        }

        public override void Visit(AddColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);
                
            handler.AddOperation(operation);
        }

        public override void Visit(DropColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
            handler.RemoveColumnNamePair(operation.ColumnName);
        }

        public override void Visit(AlterColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(AddDefaultConstraintOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(DropDefaultConstraintOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(RenameColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
            handler.ResetColumnNamePair(operation.ColumnName, operation.NewColumnName);
        }

        public override void Visit(AddPrimaryKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(DropPrimaryKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(AddForeignKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(DropForeignKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public override void Visit(RenameIndexOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandlePendingOperations();

            var table = context.Database.GetTable(operation.TableName);
            var index = table.GetIndex(operation.IndexName);

            context.HandleOperation(new DropIndexOperation(operation.TableName, operation.IndexName));
            context.HandleOperation(new CreateIndexOperation(index));
        }

        protected override void VisitDefault(MigrationOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandlePendingOperations();
            context.HandleOperation(operation);
        }

        public abstract class TableOperationHandler
        {
            private readonly SchemaQualifiedName _initialTableName;
            private readonly List<MigrationOperation> _operations;
            private readonly Dictionary<string, string> _columnNamePairs;

            protected TableOperationHandler(SchemaQualifiedName tableName, [NotNull] IEnumerable<string> columnNames)
            {
                Check.NotNull(columnNames, "columnNames");

                TableName = _initialTableName = tableName;
                _operations = new List<MigrationOperation>();
                _columnNamePairs = columnNames.ToDictionary(n => n);
            }

            protected TableOperationHandler([NotNull] TableOperationHandler other)
            {
                Check.NotNull(other, "other");

                _initialTableName = other._initialTableName;
                TableName = other.TableName;
                _operations = other._operations;
                _columnNamePairs = other._columnNamePairs;
            }

            public virtual SchemaQualifiedName InitialTableName
            {
                get { return _initialTableName; }
            }

            public virtual SchemaQualifiedName TableName { get; set; }

            public virtual IReadOnlyList<MigrationOperation> Operations
            {
                get { return _operations; }
            }

            public virtual void AddOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                _operations.Add(operation);
            }

            public virtual IReadOnlyDictionary<string, string> ColumnNamePairs
            {
                get { return _columnNamePairs; }
            }

            public virtual void ResetColumnNamePair([NotNull] string columnName, [NotNull] string newColumnName)
            {
                Check.NotEmpty(columnName, "columnName");
                Check.NotEmpty(newColumnName, "newColumnName");

                string initialName;

                if (_columnNamePairs.TryGetValue(columnName, out initialName))
                {
                    _columnNamePairs.Remove(columnName);
                }
                else
                {
                    initialName = columnName;
                }

                _columnNamePairs[newColumnName] = initialName;
            }

            public virtual void RemoveColumnNamePair([NotNull] string columnName)
            {
                Check.NotEmpty(columnName, "columnName");

                _columnNamePairs.Remove(columnName);
            }

            public abstract IEnumerable<MigrationOperation> HandleOperations([NotNull] Context context);
        }

        public class CreateTableHandler : TableOperationHandler
        {
            public CreateTableHandler([NotNull] CreateTableOperation operation)
                : base(
                    Check.NotNull(operation, "operation").Table.Name,
                    operation.Table.Columns.Select(c => c.Name))
            {
                AddOperation(operation);
            }

            public override IEnumerable<MigrationOperation> HandleOperations(Context context)
            {
                Check.NotNull(context, "context");

                context.Generator.DatabaseModelModifier.Modify(context.Database, Operations);

                var table = context.Database.GetTable(TableName);

                yield return new CreateTableOperation(table);
            }
        }

        public class AlterTableHandler : TableOperationHandler
        {
            public AlterTableHandler(SchemaQualifiedName tableName, [NotNull] IEnumerable<string> columnNames)
                : base(tableName, columnNames)
            {
            }

            public override IEnumerable<MigrationOperation> HandleOperations(Context context)
            {
                Check.NotNull(context, "context");

                return Operations;
            }
        }

        public class RebuildTableHandler : TableOperationHandler
        {
            public RebuildTableHandler(SchemaQualifiedName tableName, [NotNull] IEnumerable<string> columnNames)
                : base(tableName, columnNames)
            {
            }

            public RebuildTableHandler([NotNull] AlterTableHandler alterTableHandler)
                : base(alterTableHandler)
            {
            }

            public override IEnumerable<MigrationOperation> HandleOperations(Context context)
            {
                Check.NotNull(context, "context");

                context.Generator.DatabaseModelModifier.Modify(context.Database, Operations);

                var targetTable = context.Database.GetTable(TableName);
                var sourceTableName = InitialTableName;
                var targetColumnNames
                    = targetTable.Columns
                        .Where(c => ColumnNamePairs.ContainsKey(c.Name))
                        .Select(c => c.Name)
                        .ToArray();
                var sourceColumnNames
                    = targetColumnNames
                        .Select(n => ColumnNamePairs[n])
                        .ToArray();

                if (sourceTableName == targetTable.Name)
                {
                    sourceTableName = new SchemaQualifiedName("__mig_tmp__" + sourceTableName.Name, sourceTableName.Schema);
                    
                    yield return new RenameTableOperation(targetTable.Name, sourceTableName.Name);
                }

                yield return new CreateTableOperation(targetTable);

                yield return new CopyDataOperation(
                    sourceTableName, sourceColumnNames, targetTable.Name, targetColumnNames);

                context.AddDeferredOperation(new DropTableOperation(sourceTableName));
            }
        }

        public class Context
        {
            private readonly SQLiteMigrationOperationSqlGenerator _generator;
            private readonly List<SqlStatement> _statements = new List<SqlStatement>();
            private readonly List<TableOperationHandler> _handlers = new List<TableOperationHandler>();
            private readonly List<MigrationOperation> _deferredOperations = new List<MigrationOperation>();
            private DatabaseModel _database;

            public Context([NotNull] SQLiteMigrationOperationSqlGenerator generator)
            {
                Check.NotNull(generator, "generator");

                _generator = generator;
            }

            public virtual SQLiteMigrationOperationSqlGenerator Generator
            {
                get { return _generator; }
            }

            public virtual IReadOnlyList<SqlStatement> Statements
            {
                get
                {
                    HandlePendingOperations();

                    return _statements;
                }
            }

            protected internal virtual IList<TableOperationHandler> Handlers
            {
                get { return _handlers; }
            }

            public virtual IReadOnlyList<MigrationOperation> DeferredOperations
            {
                get { return _deferredOperations; }
            }

            public virtual DatabaseModel Database
            {
                get { return _database; }

                [param: NotNull] protected internal set { _database = value; }
            }

            public virtual TableOperationHandler GetHandler(SchemaQualifiedName tableName)
            {
                return _handlers.FirstOrDefault(h => h.TableName == tableName);
            }

            public virtual void SetHandler([NotNull] TableOperationHandler handler)
            {
                Check.NotNull(handler, "handler");

                var index = _handlers.FindIndex(h => h.TableName == handler.TableName);

                if (index >= 0)
                {
                    _handlers[index] = handler;
                }
                else
                {
                    _handlers.Add(handler);
                }
            }

            public virtual TableOperationHandler EnsureHandler(SchemaQualifiedName tableName, bool supported)
            {
                var handler = GetHandler(tableName);

                if (handler == null)
                {
                    var table = _generator.Database.TryGetTable(tableName);
                    var columnNames = table != null ? table.Columns.Select(c => c.Name) : Enumerable.Empty<string>();

                    SetHandler(handler
                        = supported
                            ? (TableOperationHandler)new AlterTableHandler(tableName, columnNames)
                            : new RebuildTableHandler(tableName, columnNames));
                }
                else if (!supported)
                {
                    var alterTableHandler = handler as AlterTableHandler;

                    if (alterTableHandler != null)
                    {
                        SetHandler(handler = new RebuildTableHandler(alterTableHandler));
                    }
                }

                return handler;
            }

            public virtual void AddDeferredOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                _deferredOperations.Add(operation);
            }

            public virtual void HandleOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                _statements.Add(_generator.Generate(operation));
                _generator.DatabaseModelModifier.Modify(_generator.Database, operation);
            }

            public virtual void HandlePendingOperations()
            {
                _database = _generator.Database.Clone();

                foreach (var operation in 
                    _handlers
                        .SelectMany(h => h.HandleOperations(this))
                        .Concat(_deferredOperations))
                {
                    _statements.Add(_generator.Generate(operation));
                    _generator.DatabaseModelModifier.Modify(_generator.Database, operation);
                }

                _handlers.Clear();
                _deferredOperations.Clear();
            }
        }
    }
}
