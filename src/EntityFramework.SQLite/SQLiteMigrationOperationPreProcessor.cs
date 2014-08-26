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
        // TODO: Figure out what needs to be done with the sequence operations.

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

            var newTableName = new SchemaQualifiedName(operation.NewTableName, operation.TableName.Schema);

            context.HandleRenameOperation(operation.TableName, operation, newTableName);
        }

        public override void Visit(MoveTableOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var newTableName = new SchemaQualifiedName(operation.TableName.Name, operation.NewSchema);

            context.HandleRenameOperation(operation.TableName, operation, newTableName);
        }

        public override void Visit(AddColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: true);
        }

        public override void Visit(DropColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(AlterColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(AddDefaultConstraintOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(DropDefaultConstraintOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(RenameColumnOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(AddPrimaryKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(DropPrimaryKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(AddForeignKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
        }

        public override void Visit(DropForeignKeyOperation operation, Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandleSubordinateOperation(operation.TableName, operation, supported: false);
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

            protected TableOperationHandler(SchemaQualifiedName tableName)
            {
                _initialTableName = tableName;
                TableName = tableName;
                _operations = new List<MigrationOperation>();
            }

            protected TableOperationHandler([NotNull] TableOperationHandler other)
            {
                Check.NotNull(other, "other");

                _initialTableName = other._initialTableName;
                TableName = other.TableName;
                _operations = new List<MigrationOperation>(other._operations);
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

            public abstract IEnumerable<MigrationOperation> HandleOperations([NotNull] Context context);
        }

        public class CreateTableHandler : TableOperationHandler
        {
            public CreateTableHandler([NotNull] CreateTableOperation operation)
                : base(operation.Table.Name)
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
            public AlterTableHandler(SchemaQualifiedName tableName)
                : base(tableName)
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
            public RebuildTableHandler(SchemaQualifiedName tableName)
                : base(tableName)
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

                var table = context.Database.GetTable(TableName);

                yield return new DropTableOperation(InitialTableName);

                yield return new CreateTableOperation(table);

                // TODO: Implement operation to move data between tables.
            }
        }

        public class Context
        {
            private readonly SQLiteMigrationOperationSqlGenerator _generator;
            private readonly List<SqlStatement> _statements = new List<SqlStatement>();
            private readonly List<TableOperationHandler> _handlers = new List<TableOperationHandler>();
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

            public virtual void HandleOperation([NotNull] MigrationOperation operation)
            {
                Check.NotNull(operation, "operation");

                _statements.Add(_generator.Generate(operation));
                _generator.DatabaseModelModifier.Modify(_generator.Database, operation);
            }

            public virtual void HandleSubordinateOperation(SchemaQualifiedName tableName,
                [NotNull] MigrationOperation operation, bool supported)
            {
                Check.NotNull(operation, "operation");

                var handler = GetHandler(tableName);

                if (handler == null)
                {
                    SetHandler(handler
                        = supported
                            ? (TableOperationHandler)new AlterTableHandler(tableName)
                            : new RebuildTableHandler(tableName));
                }
                else if (!supported)
                {
                    var alterTableHandler = handler as AlterTableHandler;

                    if (alterTableHandler != null)
                    {
                        SetHandler(handler = new RebuildTableHandler(alterTableHandler));
                    }
                }

                handler.AddOperation(operation);
            }

            public virtual void HandleRenameOperation(SchemaQualifiedName tableName,
                [NotNull] MigrationOperation operation, SchemaQualifiedName newTableName)
            {
                Check.NotNull(operation, "operation");

                var handler = GetHandler(tableName);

                if (handler == null)
                {
                    SetHandler(handler = new AlterTableHandler(tableName));
                }

                handler.TableName = newTableName;
                handler.AddOperation(operation);
            }

            public virtual void HandlePendingOperations()
            {
                _database = _generator.Database.Clone();

                foreach (var operation in _handlers.SelectMany(h => h.HandleOperations(this)))
                {
                    _statements.Add(_generator.Generate(operation));
                    _generator.DatabaseModelModifier.Modify(_generator.Database, operation);
                }

                _handlers.Clear();
            }
        }
    }
}
