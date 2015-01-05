// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public class MigrationOperationFactory
    {
        private readonly IRelationalMetadataExtensionProvider _extensionProvider;

        public MigrationOperationFactory(
            [NotNull] IRelationalMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        protected virtual RelationalNameBuilder NameBuilder
        {
            get { return ExtensionProvider.NameBuilder; }
        }

        public virtual CreateSequenceOperation CreateSequenceOperation([NotNull] ISequence target)
        {
            Check.NotNull(target, "target");

            return
                new CreateSequenceOperation(
                    NameBuilder.SchemaQualifiedSequenceName(target),
                    target.StartValue,
                    target.IncrementBy,
                    target.MinValue,
                    target.MaxValue,
                    target.Type);
        }

        public virtual CreateTableOperation CreateTableOperation([NotNull] IEntityType target)
        {
            Check.NotNull(target, "target");

            var operation = new CreateTableOperation(NameBuilder.SchemaQualifiedTableName(target));

            operation.Columns.AddRange(OrderProperties(target).Select(Column));

            var primaryKey = target.TryGetPrimaryKey();
            if (primaryKey != null)
            {
                operation.PrimaryKey = AddPrimaryKeyOperation(primaryKey);
            }

            operation.UniqueConstraints.AddRange(target.Keys.Where(key => key != primaryKey).Select(AddUniqueConstraintOperation));
            operation.ForeignKeys.AddRange(target.ForeignKeys.Select(AddForeignKeyOperation));
            operation.Indexes.AddRange(target.Indexes.Select(CreateIndexOperation));

            return operation;
        }

        public virtual Column Column([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            var extensions = ExtensionProvider.Extensions(property);

            return
                new Column(NameBuilder.ColumnName(property), property.PropertyType)
                {
                    DataType = extensions.ColumnType,
                    IsNullable = property.IsNullable,
                    DefaultValue = extensions.DefaultValue,
                    DefaultSql = extensions.DefaultExpression,
                    IsComputed = property.IsStoreComputed,
                    IsTimestamp = property.IsConcurrencyToken && property.PropertyType == typeof(byte[]),
                    MaxLength = property.MaxLength > 0 ? property.MaxLength : (int?)null
                };
        }

        public virtual AddColumnOperation AddColumnOperation([NotNull] IProperty target)
        {
            Check.NotNull(target, "target");

            return
                new AddColumnOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    Column(target));
        }

        public virtual AddDefaultConstraintOperation AddDefaultConstraintOperation([NotNull] IProperty target)
        {
            Check.NotNull(target, "target");

            var extensions = ExtensionProvider.Extensions(target);

            return
                new AddDefaultConstraintOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.ColumnName(target),
                    extensions.DefaultValue,
                    extensions.DefaultExpression);
        }

        public virtual AddPrimaryKeyOperation AddPrimaryKeyOperation([NotNull] IKey target)
        {
            Check.NotNull(target, "target");

            return
                new AddPrimaryKeyOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.KeyName(target),
                    target.Properties.Select(p => NameBuilder.ColumnName(p)).ToList(),
                    // TODO: Issue #879: Clustered is SQL Server-specific.
                    isClustered: true);
        }

        public virtual AddUniqueConstraintOperation AddUniqueConstraintOperation([NotNull] IKey target)
        {
            Check.NotNull(target, "target");

            return
                new AddUniqueConstraintOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.KeyName(target),
                    target.Properties.Select(p => NameBuilder.ColumnName(p)).ToList());
        }

        public virtual AddForeignKeyOperation AddForeignKeyOperation([NotNull] IForeignKey target)
        {
            Check.NotNull(target, "target");

            return
                new AddForeignKeyOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.ForeignKeyName(target),
                    target.Properties.Select(p => NameBuilder.ColumnName(p)).ToList(),
                    NameBuilder.SchemaQualifiedTableName(target.ReferencedEntityType),
                    target.ReferencedProperties.Select(p => NameBuilder.ColumnName(p)).ToList(),
                    // TODO: Issue #333: Cascading behaviors not supported.
                    cascadeDelete: false);
        }

        public virtual CreateIndexOperation CreateIndexOperation([NotNull] IIndex target)
        {
            Check.NotNull(target, "target");

            return
                new CreateIndexOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.IndexName(target),
                    target.Properties.Select(p => NameBuilder.ColumnName(p)).ToList(),
                    target.IsUnique,
                    // TODO: Issue #879: Clustered is SQL Server-specific.
                    isClustered: false);
        }

        public virtual DropSequenceOperation DropSequenceOperation([NotNull] ISequence source)
        {
            Check.NotNull(source, "source");

            return new DropSequenceOperation(NameBuilder.SchemaQualifiedSequenceName(source));
        }

        public virtual DropTableOperation DropTableOperation([NotNull] IEntityType source)
        {
            Check.NotNull(source, "source");

            return new DropTableOperation(NameBuilder.SchemaQualifiedTableName(source));
        }

        public virtual DropColumnOperation DropColumnOperation([NotNull] IProperty source)
        {
            Check.NotNull(source, "source");

            return 
                new DropColumnOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType), 
                    NameBuilder.ColumnName(source));
        }

        public virtual DropDefaultConstraintOperation DropDefaultConstraintOperation([NotNull] IProperty source)
        {
            Check.NotNull(source, "source");

            return
                new DropDefaultConstraintOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType),
                    NameBuilder.ColumnName(source));
        }

        public virtual DropPrimaryKeyOperation DropPrimaryKeyOperation([NotNull] IKey source)
        {
            Check.NotNull(source, "source");

            return
                new DropPrimaryKeyOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType),
                    NameBuilder.KeyName(source));
        }

        public virtual DropUniqueConstraintOperation DropUniqueConstraintOperation([NotNull] IKey source)
        {
            Check.NotNull(source, "source");

            return
                new DropUniqueConstraintOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType),
                    NameBuilder.KeyName(source));
        }

        public virtual DropForeignKeyOperation DropForeignKeyOperation([NotNull] IForeignKey source)
        {
            Check.NotNull(source, "source");

            return
                new DropForeignKeyOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType),
                    NameBuilder.ForeignKeyName(source));
        }

        public virtual DropIndexOperation DropIndexOperation([NotNull] IIndex source)
        {
            Check.NotNull(source, "source");

            return
                new DropIndexOperation(
                    NameBuilder.SchemaQualifiedTableName(source.EntityType),
                    NameBuilder.IndexName(source));
        }

        public virtual MoveSequenceOperation MoveSequenceOperation([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new MoveSequenceOperation(
                    NameBuilder.SchemaQualifiedSequenceName(source), 
                    NameBuilder.SequenceSchema(target));
        }

        public virtual MoveTableOperation MoveTableOperation([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            return
                new MoveTableOperation(
                    NameBuilder.SchemaQualifiedTableName(source),
                    NameBuilder.TableSchema(target));
        }

        public virtual RenameSequenceOperation RenameSequenceOperation([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the schema of the target sequence because the differ handles 
            // RenameSequenceOperation after MoveSequenceOperation.
            return 
                new RenameSequenceOperation(
                    new SchemaQualifiedName(
                        NameBuilder.SequenceName(source), 
                        NameBuilder.SequenceSchema(target)), 
                    NameBuilder.SequenceName(target));
        }

        public virtual RenameTableOperation RenameTableOperation([NotNull] IEntityType source, [NotNull] IEntityType target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the schema of the target sequence because the differ handles 
            // RenameTableOperation after MoveTableOperation.
            return 
                new RenameTableOperation(
                    new SchemaQualifiedName(
                        NameBuilder.TableName(source), 
                        NameBuilder.TableSchema(target)),
                    NameBuilder.TableName(target));
        }

        public virtual RenameColumnOperation RenameColumnOperation([NotNull] IProperty source, [NotNull] IProperty target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the full name of the target table because the differ handles 
            // RenameColumnOperation after MoveTableOperation and RenameTableOperation.
            return 
                new RenameColumnOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType), 
                    NameBuilder.ColumnName(source), 
                    NameBuilder.ColumnName(target));
        }

        public virtual RenameIndexOperation RenameIndexOperation([NotNull] IIndex source, [NotNull] IIndex target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the full name of the target table because the differ handles 
            // RenameIndexOperation after MoveTableOperation and RenameTableOperation.
            return
                new RenameIndexOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    NameBuilder.IndexName(source),
                    NameBuilder.IndexName(target));
        }

        public virtual AlterSequenceOperation AlterSequenceOperation([NotNull] ISequence source, [NotNull] ISequence target)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the full name of the target sequence because the differ handles 
            // AlterSequenceOperation after MoveSequenceOperation and RenameSequenceOperation.
            return
                new AlterSequenceOperation(
                    NameBuilder.SchemaQualifiedSequenceName(target),
                    target.IncrementBy);
        }

        public virtual AlterColumnOperation AlterColumnOperation([NotNull] IProperty source, [NotNull] IProperty target, bool isDestructiveChange)
        {
            Check.NotNull(source, "source");
            Check.NotNull(target, "target");

            // NOTE: Must use the full name of the target table because the differ handles 
            // AlterColumnOperation after MoveTableOperation and RenameTableOperation.
            return 
                new AlterColumnOperation(
                    NameBuilder.SchemaQualifiedTableName(target.EntityType),
                    Column(target),
                    isDestructiveChange);
        }

        public virtual IEnumerable<IProperty> OrderProperties([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            var primaryKey = entityType.TryGetPrimaryKey();

            var primaryKeyProperties
                = primaryKey != null
                    ? primaryKey.Properties.ToArray()
                    : new IProperty[0];

            var foreignKeyProperties
                = entityType.ForeignKeys
                    .SelectMany(fk => fk.Properties)
                    .Except(primaryKeyProperties)
                    .Distinct()
                    .ToArray();

            var otherProperties
                = entityType.Properties
                    .Except(primaryKeyProperties.Concat(foreignKeyProperties))
                    .OrderBy(p => NameBuilder.ColumnName(p))
                    .ToArray();

            return primaryKeyProperties
                .Concat(otherProperties)
                .Concat(foreignKeyProperties);
        }
    }
}
