// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationProcessor : MigrationOperationProcessor
    {
        public SqlServerMigrationOperationProcessor(
            [NotNull] SqlServerMetadataExtensionProvider extensionProvider,
            [NotNull] SqlServerTypeMapper typeMapper,
            [NotNull] SqlServerMigrationOperationFactory operationFactory)
            : base(
                extensionProvider,
                typeMapper,
                operationFactory)
        {
        }

        public virtual new SqlServerMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqlServerMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public virtual new SqlServerTypeMapper TypeMapper
        {
            get { return (SqlServerTypeMapper)base.TypeMapper; }
        }

        public virtual new SqlServerMigrationOperationFactory OperationFactory
        {
            get { return (SqlServerMigrationOperationFactory)base.OperationFactory; }
        }

        public override IReadOnlyList<MigrationOperation> Process(
            MigrationOperationCollection operations, IModel sourceModel, IModel targetModel)
        {
            Check.NotNull(operations, "operations");
            Check.NotNull(sourceModel, "sourceModel");
            Check.NotNull(targetModel, "targetModel");

            var context = new Context(operations, sourceModel, targetModel);

            foreach (var operation in operations.Get<DropTableOperation>())
            {
                Process(operation, context);
            }

            foreach (var operation in operations.Get<DropColumnOperation>())
            {
                Process(operation, context);
            }

            foreach (var operation in operations.Get<AlterColumnOperation>())
            {
                Process(operation, context);
            }

            return context.Operations.GetAll();
        }

        protected virtual void Process(DropTableOperation dropTableOperation, Context context)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(context, "context");

            var entityType = context.SourceModel.EntityTypes.Single(
                t => NameBuilder.SchemaQualifiedTableName(t) == dropTableOperation.TableName);

            foreach (var foreignKey in context.SourceModel.EntityTypes
                    .SelectMany(t => t.ForeignKeys)
                    .Where(fk => ReferenceEquals(fk.ReferencedEntityType, entityType)))
            {
                context.Operations.Add(OperationFactory.DropForeignKeyOperation(foreignKey),
                    (x, y) => x.TableName == y.TableName && x.ForeignKeyName == y.ForeignKeyName);
            }
        }

        protected virtual void Process(DropColumnOperation dropColumnOperation, Context context)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(context, "context");

            var entityType = context.SourceModel.EntityTypes.Single(
                t => NameBuilder.SchemaQualifiedTableName(t) == dropColumnOperation.TableName);
            var property = entityType.Properties.Single(
                p => NameBuilder.ColumnName(p) == dropColumnOperation.ColumnName);
            var extensions = property.SqlServer();

            if (extensions.DefaultValue != null || extensions.DefaultExpression != null)
            {
                context.Operations.Add(OperationFactory.DropDefaultConstraintOperation(property));
            }
        }

        protected virtual void Process(AlterColumnOperation alterColumnOperation, Context context)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(context, "context");

            var entityType = context.SourceModel.EntityTypes.Single(
                t => NameBuilder.SchemaQualifiedTableName(t) == alterColumnOperation.TableName);
            var property = entityType.Properties.Single(
                p => NameBuilder.ColumnName(p) == alterColumnOperation.NewColumn.Name);
            var extensions = property.SqlServer();
            var newColumn = alterColumnOperation.NewColumn;

            string dataType, newDataType;
            GetDataTypes(entityType, property, newColumn, context, out dataType, out newDataType);

            var primaryKey = entityType.TryGetPrimaryKey();
            if (primaryKey != null
                && primaryKey.Properties.Any(p => ReferenceEquals(p, property)))
            {
                if (context.Operations.Add(OperationFactory.DropPrimaryKeyOperation(primaryKey),
                    (x, y) => x.TableName == y.TableName && x.PrimaryKeyName == y.PrimaryKeyName))
                {
                    context.Operations.Add(OperationFactory.AddPrimaryKeyOperation(primaryKey));
                }
            }

            // TODO: Changing the length of a variable-length column used in a UNIQUE constraint is allowed.
            foreach (var uniqueConstraint in entityType.Keys.Where(k => k != primaryKey)
                .Where(uc => uc.Properties.Any(p => ReferenceEquals(p, property))))
            {
                if (context.Operations.Add(OperationFactory.DropUniqueConstraintOperation(uniqueConstraint),
                    (x, y) => x.TableName == y.TableName && x.UniqueConstraintName == y.UniqueConstraintName))
                {
                    context.Operations.Add(OperationFactory.AddUniqueConstraintOperation(uniqueConstraint));
                }
            }

            foreach (var foreignKey in entityType.ForeignKeys
                .Where(fk => fk.Properties.Any(p => ReferenceEquals(p, property)))
                .Concat(context.SourceModel.EntityTypes
                    .SelectMany(t => t.ForeignKeys)
                    .Where(fk => fk.ReferencedProperties.Any(p => ReferenceEquals(p, property)))))
            {
                if (context.Operations.Add(OperationFactory.DropForeignKeyOperation(foreignKey),
                    (x, y) => x.TableName == y.TableName && x.ForeignKeyName == y.ForeignKeyName))
                {
                    context.Operations.Add(OperationFactory.AddForeignKeyOperation(foreignKey));
                }
            }

            if (dataType != newDataType
                || ((string.Equals(dataType, "varchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "nvarchar", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(dataType, "varbinary", StringComparison.OrdinalIgnoreCase))
                    && newColumn.MaxLength > property.MaxLength))
            {
                foreach (var index in entityType.Indexes
                    .Where(ix => ix.Properties.Any(p => ReferenceEquals(p, property))))
                {
                    if (context.Operations.Add(OperationFactory.DropIndexOperation(index),
                        (x, y) => x.TableName == y.TableName && x.IndexName == y.IndexName))
                    {
                        context.Operations.Add(OperationFactory.CreateIndexOperation(index));
                    }
                }
            }

            if (!property.IsStoreComputed
                && (extensions.DefaultValue != null || extensions.DefaultExpression != null))
            {
                context.Operations.Add(OperationFactory.DropDefaultConstraintOperation(property));
            }

            if (property.IsConcurrencyToken
                || property.IsStoreComputed != alterColumnOperation.NewColumn.IsComputed)
            {
                context.Operations.Remove(alterColumnOperation);
                context.Operations.Add(OperationFactory.DropColumnOperation(property));
                context.Operations.Add(new AddColumnOperation(
                    alterColumnOperation.TableName, alterColumnOperation.NewColumn));
            }
        }

        protected virtual void GetDataTypes(
            [NotNull] IEntityType entityType, [NotNull] IProperty property, [NotNull] Column newColumn, [NotNull] Context context,
            out string dataType, out string newDataType)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(property, "property");
            Check.NotNull(newColumn, "newColumn");
            Check.NotNull(context, "context");

            var isKey = property.IsKey() || property.IsForeignKey();
            var extensions = property.SqlServer();

            dataType
                = TypeMapper.GetTypeMapping(
                    extensions.ColumnType, NameBuilder.ColumnName(property), property.PropertyType, isKey, property.IsConcurrencyToken)
                    .StoreTypeName;
            newDataType
                = TypeMapper.GetTypeMapping(
                    newColumn.DataType, newColumn.Name, newColumn.ClrType, isKey, newColumn.IsTimestamp)
                    .StoreTypeName;
        }

        protected class Context
        {
            private readonly MigrationOperationCollection _operations;
            private readonly IModel _sourceModel;
            private readonly IModel _targetModel;            

            public Context(
                [NotNull] MigrationOperationCollection operations,
                [NotNull] IModel sourceModel,
                [NotNull] IModel targetModel)
            {
                Check.NotNull(operations, "operations");
                Check.NotNull(sourceModel, "sourceModel");
                Check.NotNull(targetModel, "targetModel");

                _operations = operations;
                _sourceModel = sourceModel;
                _targetModel = targetModel;                
            }

            public MigrationOperationCollection Operations
            {
                get { return _operations; }
            }

            public IModel SourceModel
            {
                get { return _sourceModel; }
            }

            public IModel TargetModel
            {
                get { return _targetModel; }
            }
        }
    }
}
