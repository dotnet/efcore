// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Metadata;
using Microsoft.Data.Entity.SQLite.Migrations;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationProcessor : MigrationOperationProcessor
    {
        public SQLiteMigrationOperationProcessor(
            [NotNull] SQLiteMetadataExtensionProvider extensionProvider,
            [NotNull] SQLiteTypeMapper typeMapper,
            [NotNull] SQLiteMigrationOperationFactory operationFactory)
            : base(
                extensionProvider,
                typeMapper,
                operationFactory)
        {
        }

        public virtual new SQLiteMetadataExtensionProvider ExtensionProvider
        {
            get { return (SQLiteMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public virtual new SQLiteTypeMapper TypeMapper
        {
            get { return (SQLiteTypeMapper)base.TypeMapper; }
        }

        public virtual new SQLiteMigrationOperationFactory OperationFactory
        {
            get { return (SQLiteMigrationOperationFactory)base.OperationFactory; }
        }

        public override IReadOnlyList<MigrationOperation> Process(            
            MigrationOperationCollection operations,
            IModel sourceModel,
            IModel targetModel)
        {            
            Check.NotNull(operations, "operations");
            Check.NotNull(sourceModel, "sourceModel");
            Check.NotNull(targetModel, "targetModel");

            var context = new Context(this, sourceModel, targetModel);

            foreach (var operation in operations.GetAll())
            {
                Process((dynamic)operation, context);
            }

            return context.Operations;
        }

        public virtual void Process([NotNull] CreateTableOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.GetHandler(operation.TableName);

            if (handler != null)
            {
                context.HandlePendingOperations();
            }

            context.SetHandler(new CreateTableHandler(operation));
        }

        public virtual void Process([NotNull] RenameTableOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);

            handler.AddOperation(operation);
            handler.TableName = new SchemaQualifiedName(operation.NewTableName, operation.TableName.Schema);
        }

        public virtual void Process([NotNull] MoveTableOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);

            handler.AddOperation(operation);
            handler.TableName = new SchemaQualifiedName(operation.TableName.Name, operation.NewSchema);
        }

        public virtual void Process([NotNull] AddColumnOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: true);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] DropColumnOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
            handler.RemoveColumnNamePair(operation.ColumnName);
        }

        public virtual void Process([NotNull] AlterColumnOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] AddDefaultConstraintOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] DropDefaultConstraintOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] RenameColumnOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
            handler.ResetColumnNamePair(operation.ColumnName, operation.NewColumnName);
        }

        public virtual void Process([NotNull] AddPrimaryKeyOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] DropPrimaryKeyOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] AddForeignKeyOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] DropForeignKeyOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            var handler = context.EnsureHandler(operation.TableName, supported: false);

            handler.AddOperation(operation);
        }

        public virtual void Process([NotNull] RenameIndexOperation operation, [NotNull] Context context)
        {
            Check.NotNull(operation, "operation");
            Check.NotNull(context, "context");

            context.HandlePendingOperations();

            var entitType = context.SourceModel.EntityTypes.Single(
                t => NameBuilder.SchemaQualifiedTableName(t) == operation.TableName);
            var index = entitType.Indexes.Single(
                ix => NameBuilder.IndexName(ix) == operation.IndexName);

            context.HandleOperation(OperationFactory.DropIndexOperation(index));
            context.HandleOperation(OperationFactory.CreateIndexOperation(index));
        }

        public virtual void Process([NotNull] MigrationOperation operation, [NotNull] Context context)
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
                    Check.NotNull(operation, "operation").TableName,
                    operation.Columns.Select(c => c.Name))
            {
                base.AddOperation(operation);
            }

            public override void AddOperation(MigrationOperation operation)
            {
                // TODO: Currently the ModelDiffer outputs instances of AddForeignKeyOperation for each foreign key of 
                // a table to be created. These operations need to be ignored because the SQLite SQL generator includes 
                // the foreign key definitions within the CREATE TABLE statement. Figure out if there is a cleaner solution.

                var createTableOperation = (CreateTableOperation)Operations[0];

                var addForeignKeyOperation = operation as AddForeignKeyOperation;
                if (addForeignKeyOperation != null)
                {
                    if (createTableOperation.ForeignKeys.All(addFkOp => addFkOp != addForeignKeyOperation))
                    {
                        throw new InvalidOperationException();
                    }

                    return;
                }

                throw new InvalidOperationException();
            }

            public override IEnumerable<MigrationOperation> HandleOperations(Context context)
            {
                Check.NotNull(context, "context");

                yield return Operations[0];
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

                var nameBuilder = context.Processor.NameBuilder;
                var operationFactory = context.Processor.OperationFactory;

                var targetTableName = TableName;
                var targetEntityType = context.TargetModel.EntityTypes.Single(
                    t => nameBuilder.SchemaQualifiedTableName(t) == targetTableName);
                var sourceTableName = InitialTableName;
                var targetColumnNames
                    = operationFactory.OrderProperties(targetEntityType)
                        .Where(p => ColumnNamePairs.ContainsKey(nameBuilder.ColumnName(p)))
                        .Select(nameBuilder.ColumnName)
                        .ToList();
                var sourceColumnNames
                    = targetColumnNames
                        .Select(n => ColumnNamePairs[n])
                        .ToList();

                if (sourceTableName == targetTableName)
                {
                    sourceTableName = new SchemaQualifiedName("__mig_tmp__" + sourceTableName.Name, sourceTableName.Schema);

                    yield return new RenameTableOperation(targetTableName, sourceTableName.Name);
                }

                yield return operationFactory.CreateTableOperation(targetEntityType);

                yield return new CopyDataOperation(sourceTableName, sourceColumnNames, targetTableName, targetColumnNames);

                context.AddDeferredOperation(new DropTableOperation(sourceTableName));
            }
        }

        public class Context
        {
            private readonly SQLiteMigrationOperationProcessor _processor;
            private readonly IModel _sourceModel;
            private readonly IModel _targetModel;
            private readonly List<MigrationOperation> _operations = new List<MigrationOperation>();
            private readonly List<TableOperationHandler> _handlers = new List<TableOperationHandler>();
            private readonly List<MigrationOperation> _deferredOperations = new List<MigrationOperation>();

            public Context([NotNull] SQLiteMigrationOperationProcessor processor, [NotNull] IModel sourceModel, [NotNull] IModel targetModel)
            {
                Check.NotNull(processor, "processor");
                Check.NotNull(sourceModel, "sourceModel");
                Check.NotNull(targetModel, "targetModel");

                _processor = processor;
                _sourceModel = sourceModel;
                _targetModel = targetModel;
            }

            public virtual SQLiteMigrationOperationProcessor Processor
            {
                get { return _processor; }
            }

            public virtual IModel SourceModel
            {
                get { return _sourceModel; }
            }

            public virtual IModel TargetModel
            {
                get { return _targetModel; }
            }

            public virtual IReadOnlyList<MigrationOperation> Operations
            {
                get
                {
                    HandlePendingOperations();

                    return _operations;
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
                    var entityType = SourceModel.EntityTypes.Single(
                        t => Processor.NameBuilder.SchemaQualifiedTableName(t) == tableName);
                    var columnNames = entityType.Properties.Select(p => Processor.NameBuilder.ColumnName(p));

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

                _operations.Add(operation);
            }

            public virtual void HandlePendingOperations()
            {
                foreach (var operation in 
                    _handlers
                        .SelectMany(h => h.HandleOperations(this))
                        .Concat(_deferredOperations))
                {
                    _operations.Add(operation);
                }

                _handlers.Clear();
                _deferredOperations.Clear();
            }
        }
    }
}
