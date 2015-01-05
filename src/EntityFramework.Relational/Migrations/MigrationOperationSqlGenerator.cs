// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Relational.Migrations.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    /// <summary>
    ///     Default migration operation SQL generator, outputs best-effort ANSI-99 compliant SQL.
    /// </summary>
    // TODO: For simplicity we could rename the "abcOperation" parameters to "operation".
    public abstract class MigrationOperationSqlGenerator
    {
        // TODO: Check whether the following formats ar SqlServer specific or not and move
        // to SqlServer provider if they are.
        internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        internal const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private readonly IRelationalMetadataExtensionProvider _extensionProvider;
        private readonly RelationalTypeMapper _typeMapper;
        private IModel _targetModel;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected MigrationOperationSqlGenerator()
        {
        }

        protected MigrationOperationSqlGenerator(
            [NotNull] IRelationalMetadataExtensionProvider extensionProvider,
            [NotNull] RelationalTypeMapper typeMapper)
        {
            Check.NotNull(extensionProvider, "extensionProvider");
            Check.NotNull(typeMapper, "typeMapper");

            _extensionProvider = extensionProvider;
            _typeMapper = typeMapper;
        }

        public virtual IRelationalMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        protected virtual RelationalNameBuilder NameBuilder
        {
            get { return ExtensionProvider.NameBuilder; }
        }

        public virtual RelationalTypeMapper TypeMapper
        {
            get { return _typeMapper; }
        }

        public virtual IModel TargetModel
        {
            get { return _targetModel; }
            [param: NotNull] set { _targetModel = Check.NotNull(value, "value"); }
        }

        public virtual string StatementSeparator
        {
            get
            {
                return ";";
            }
        }

        public virtual IEnumerable<SqlBatch> Generate([NotNull] IEnumerable<MigrationOperation> migrationOperations)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            var batchBuilder = new SqlBatchBuilder();
            var migrationOperationsList = migrationOperations.ToList();
            for (var i = 0; i < migrationOperationsList.Count; i++)
            {
                if (i > 0)
                {
                    batchBuilder.AppendLine(StatementSeparator);
                }

                migrationOperationsList[i].GenerateSql(this, batchBuilder);
            }

            batchBuilder.EndBatch();
            
            return batchBuilder.SqlBatches;
        }

        public virtual IEnumerable<SqlBatch> Generate([NotNull] MigrationOperation operation)
        {
            Check.NotNull(operation, "operation");

            var batchBuilder = new SqlBatchBuilder();
            operation.GenerateSql(this, batchBuilder);
            batchBuilder.EndBatch();

            return batchBuilder.SqlBatches;
        }

        public virtual void Generate([NotNull] CreateDatabaseOperation createDatabaseOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("CREATE DATABASE ")
                .Append(DelimitIdentifier(createDatabaseOperation.DatabaseName));
        }

        public virtual void Generate([NotNull] DropDatabaseOperation dropDatabaseOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("DROP DATABASE ")
                .Append(DelimitIdentifier(dropDatabaseOperation.DatabaseName));
        }

        public virtual void Generate([NotNull] CreateSequenceOperation createSequenceOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var dataType = _typeMapper.GetTypeMapping(
                null, createSequenceOperation.SequenceName, createSequenceOperation.Type, 
                isKey: false, isConcurrencyToken: false).StoreTypeName;

            EnsureSchema(createSequenceOperation.SequenceName.Schema, batchBuilder);

            batchBuilder
                .Append("CREATE SEQUENCE ")
                .Append(DelimitIdentifier(createSequenceOperation.SequenceName))
                .Append(" AS ")
                .Append(dataType)
                .Append(" START WITH ")
                .Append(createSequenceOperation.StartValue)
                .Append(" INCREMENT BY ")
                .Append(createSequenceOperation.IncrementBy);
        }

        public virtual void Generate([NotNull] DropSequenceOperation dropSequenceOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("DROP SEQUENCE ")
                .Append(DelimitIdentifier(dropSequenceOperation.SequenceName));
        }

        public abstract void Generate([NotNull] RenameSequenceOperation renameSequenceOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public abstract void Generate([NotNull] MoveSequenceOperation moveSequenceOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public virtual void Generate([NotNull] AlterSequenceOperation alterSequenceOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(alterSequenceOperation, "alterSequenceOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER SEQUENCE ")
                .Append(DelimitIdentifier(alterSequenceOperation.SequenceName))
                .Append(" INCREMENT BY ")
                .Append(alterSequenceOperation.NewIncrementBy);
        }

        public virtual void Generate([NotNull] CreateTableOperation createTableOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            EnsureSchema(createTableOperation.TableName.Schema, batchBuilder);

            batchBuilder
                .Append("CREATE TABLE ")
                .Append(DelimitIdentifier(createTableOperation.TableName))
                .AppendLine(" (");

            using (batchBuilder.Indent())
            {
                GenerateColumns(createTableOperation, batchBuilder);

                GenerateTableConstraints(createTableOperation, batchBuilder);
            }

            batchBuilder
                .AppendLine()
                .Append(")");
        }

        protected virtual void GenerateTableConstraints([NotNull] CreateTableOperation createTableOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var addPrimaryKeyOperation = createTableOperation.PrimaryKey;

            if (addPrimaryKeyOperation != null)
            {
                batchBuilder.AppendLine(",");

                GeneratePrimaryKey(addPrimaryKeyOperation, batchBuilder);
            }

            foreach (var addUniqueConstraintOperation in createTableOperation.UniqueConstraints)
            {
                batchBuilder.AppendLine(",");

                GenerateUniqueConstraint(addUniqueConstraintOperation, batchBuilder);
            }
        }

        public virtual void Generate([NotNull] DropTableOperation dropTableOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("DROP TABLE ")
                .Append(DelimitIdentifier(dropTableOperation.TableName));
        }

        public abstract void Generate([NotNull] RenameTableOperation renameTableOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public abstract void Generate([NotNull] MoveTableOperation moveTableOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public virtual void Generate([NotNull] AddColumnOperation addColumnOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addColumnOperation.TableName))
                .Append(" ADD ");

            GenerateColumn(addColumnOperation.TableName, addColumnOperation.Column, batchBuilder);
        }

        public virtual void Generate([NotNull] DropColumnOperation dropColumnOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropColumnOperation.TableName))
                .Append(" DROP COLUMN ")
                .Append(DelimitIdentifier(dropColumnOperation.ColumnName));
        }

        public virtual void Generate([NotNull] AlterColumnOperation alterColumnOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var newColumn = alterColumnOperation.NewColumn;

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(alterColumnOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(newColumn.Name))
                .Append(" ");

            GenerateColumnType(alterColumnOperation.TableName, newColumn, batchBuilder);

            batchBuilder
                .Append(newColumn.IsNullable ? " NULL" : " NOT NULL");
        }

        public virtual void Generate([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.ColumnName))
                .Append(" SET DEFAULT ");

            batchBuilder.Append(addDefaultConstraintOperation.DefaultSql ?? GenerateLiteral((dynamic)addDefaultConstraintOperation.DefaultValue));
        }

        public virtual void Generate([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.ColumnName))
                .Append(" DROP DEFAULT");
        }

        public abstract void Generate([NotNull] RenameColumnOperation renameColumnOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public virtual void Generate([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.TableName))
                .Append(" ADD ");

            GeneratePrimaryKey(addPrimaryKeyOperation, batchBuilder);
        }

        public virtual void Generate([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.PrimaryKeyName));
        }

        public virtual void Generate([NotNull] AddUniqueConstraintOperation addUniqueConstraintOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addUniqueConstraintOperation, "addUniqueConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addUniqueConstraintOperation.TableName))
                .Append(" ADD ");

            GenerateUniqueConstraint(addUniqueConstraintOperation, batchBuilder);
        }

        public virtual void Generate([NotNull] DropUniqueConstraintOperation dropUniqueConstraintOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropUniqueConstraintOperation, "dropUniqueConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropUniqueConstraintOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropUniqueConstraintOperation.UniqueConstraintName));
        }

        public virtual void Generate([NotNull] AddForeignKeyOperation addForeignKeyOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addForeignKeyOperation.TableName))
                .Append(" ADD ");

            GenerateForeignKey(addForeignKeyOperation, batchBuilder);
        }

        public virtual void Generate([NotNull] DropForeignKeyOperation dropForeignKeyOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.ForeignKeyName));
        }

        public virtual void Generate([NotNull] CreateIndexOperation createIndexOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder.Append("CREATE");

            if (createIndexOperation.IsUnique)
            {
                batchBuilder.Append(" UNIQUE");
            }

            // TODO: Move to SqlServer
            if (createIndexOperation.IsClustered)
            {
                batchBuilder.Append(" CLUSTERED");
            }

            batchBuilder
                .Append(" INDEX ")
                .Append(DelimitIdentifier(createIndexOperation.IndexName))
                .Append(" ON ")
                .Append(DelimitIdentifier(createIndexOperation.TableName))
                .Append(" (")
                .Append(createIndexOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");
        }

        public virtual void Generate([NotNull] DropIndexOperation dropIndexOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("DROP INDEX ")
                .Append(DelimitIdentifier(dropIndexOperation.IndexName));
        }

        public abstract void Generate([NotNull] RenameIndexOperation renameIndexOperation, [NotNull] SqlBatchBuilder batchBuilder);

        public virtual void Generate([NotNull] CopyDataOperation copyDataOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(copyDataOperation, "copyDataOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("INSERT INTO ")
                .Append(DelimitIdentifier(copyDataOperation.TargetTableName))
                .Append(" ( ")
                .Append(copyDataOperation.TargetColumnNames.Select(DelimitIdentifier).Join())
                .AppendLine(" )");

            using (batchBuilder.Indent())
            {
                batchBuilder
                    .Append("SELECT ")
                    .Append(copyDataOperation.SourceColumnNames.Select(DelimitIdentifier).Join())
                    .Append(" FROM ")
                    .Append(DelimitIdentifier(copyDataOperation.SourceTableName));
            }
        }

        public virtual void Generate([NotNull] SqlOperation sqlOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(sqlOperation, "sqlOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder.Append(sqlOperation.Sql, sqlOperation.SuppressTransaction);
        }

        protected abstract void EnsureSchema([CanBeNull] string schema, [NotNull] SqlBatchBuilder batchBuilder);

        protected virtual void CreateSchema([NotNull] string schema, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotEmpty(schema, "schema");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("EXECUTE(")
                .Append(GenerateLiteral("CREATE SCHEMA " + DelimitIdentifier(schema)))
                .Append(")");
        }

        public virtual void GenerateColumnType(
            SchemaQualifiedName tableName, [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(batchBuilder, "batchBuilder");

            if (!string.IsNullOrEmpty(column.DataType))
            {
                batchBuilder.Append(column.DataType);
                return;
            }

            var entityType = TargetModel.EntityTypes.Single(t => NameBuilder.SchemaQualifiedTableName(t) == tableName);
            var property = entityType.Properties.Single(p => NameBuilder.ColumnName(p) == column.Name);
            var isKey = property.IsKey() || property.IsForeignKey();

            batchBuilder.Append(_typeMapper.GetTypeMapping(column.DataType, 
                column.Name, column.ClrType, isKey, column.IsTimestamp).StoreTypeName);
        }

        public virtual string GenerateLiteral([NotNull] object value)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        public virtual string GenerateLiteral(bool value)
        {
            return value ? "1" : "0";
        }

        public virtual string GenerateLiteral([NotNull] string value)
        {
            Check.NotNull(value, "value");

            return "'" + EscapeLiteral(value) + "'";
        }

        public virtual string GenerateLiteral(Guid value)
        {
            return "'" + value + "'";
        }

        public virtual string GenerateLiteral(DateTime value)
        {
            return "'" + value.ToString(DateTimeFormat, CultureInfo.InvariantCulture) + "'";
        }

        public virtual string GenerateLiteral(DateTimeOffset value)
        {
            return "'" + value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture) + "'";
        }

        public virtual string GenerateLiteral(TimeSpan value)
        {
            return "'" + value + "'";
        }

        public virtual string GenerateLiteral([NotNull] byte[] value)
        {
            Check.NotNull(value, "value");

            var stringBuilder = new StringBuilder("0x");

            foreach (var @byte in value)
            {
                stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }

            return stringBuilder.ToString();
        }

        public virtual string DelimitIdentifier(SchemaQualifiedName schemaQualifiedName)
        {
            return
                (schemaQualifiedName.IsSchemaQualified
                    ? DelimitIdentifier(schemaQualifiedName.Schema) + "."
                    : string.Empty)
                + DelimitIdentifier(schemaQualifiedName.Name);
        }

        public virtual string DelimitIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "\"" + EscapeIdentifier(identifier) + "\"";
        }

        public virtual string EscapeIdentifier([NotNull] string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("\"", "\"\"");
        }

        public virtual string EscapeLiteral([NotNull] string literal)
        {
            Check.NotNull(literal, "literal");

            return literal.Replace("'", "''");
        }

        protected virtual void GenerateColumns(
            [NotNull] CreateTableOperation createTableOperation, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            var columns = createTableOperation.Columns;
            if (columns.Count == 0)
            {
                return;
            }

            GenerateColumn(createTableOperation.TableName, columns[0], batchBuilder);

            for (var i = 1; i < columns.Count; i++)
            {
                batchBuilder.AppendLine(",");

                GenerateColumn(createTableOperation.TableName, columns[i], batchBuilder);
            }
        }

        protected virtual void GenerateColumn(
            SchemaQualifiedName tableName, [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(batchBuilder, "batchBuilder");

            if (column.IsComputed && column.DefaultSql != null)
            {
                GenerateComputedColumn(tableName, column, batchBuilder);

                return;
            }

            batchBuilder
                .Append(DelimitIdentifier(column.Name))
                .Append(" ");

            GenerateColumnType(tableName, column, batchBuilder);

            GenerateNullConstraint(tableName, column, batchBuilder);

            GenerateColumnTraits(tableName, column, batchBuilder);

            GenerateDefaultConstraint(tableName, column, batchBuilder);
        }

        protected virtual void GenerateComputedColumn(SchemaQualifiedName tableName,
            [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
        }

        protected virtual void GenerateColumnTraits(SchemaQualifiedName tableName,
            [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
        }

        protected virtual void GenerateNullConstraint(
            SchemaQualifiedName tableName, [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(batchBuilder, "batchBuilder");

            if (!column.IsNullable)
            {
                batchBuilder.Append(" NOT NULL");
            }
        }

        protected virtual void GenerateDefaultConstraint(
            SchemaQualifiedName tableName, [NotNull] Column column, [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(batchBuilder, "batchBuilder");

            if (column.DefaultSql != null)
            {
                batchBuilder
                    .Append(" DEFAULT ")
                    .Append(column.DefaultSql);
            }
            else if (column.DefaultValue != null)
            {
                batchBuilder
                    .Append(" DEFAULT ")
                    .Append(GenerateLiteral((dynamic)column.DefaultValue));
            }
        }

        protected virtual void GeneratePrimaryKey(
            [NotNull] AddPrimaryKeyOperation primaryKeyOperation,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(primaryKeyOperation, "primaryKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(primaryKeyOperation.PrimaryKeyName))
                .Append(" PRIMARY KEY");

            GeneratePrimaryKeyTraits(primaryKeyOperation, batchBuilder);

            batchBuilder
                .Append(" (")
                .Append(primaryKeyOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");
        }

        protected virtual void GeneratePrimaryKeyTraits(
            [NotNull] AddPrimaryKeyOperation primaryKeyOperation,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
        }

        protected virtual void GenerateUniqueConstraint(
            [NotNull] AddUniqueConstraintOperation uniqueConstraintOperation,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(uniqueConstraintOperation, "uniqueConstraintOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(uniqueConstraintOperation.UniqueConstraintName))
                .Append(" UNIQUE (")
                .Append(uniqueConstraintOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");
        }

        protected virtual void GenerateForeignKey(
            [NotNull] AddForeignKeyOperation foreignKeyOperation,
            [NotNull] SqlBatchBuilder batchBuilder)
        {
            Check.NotNull(foreignKeyOperation, "foreignKeyOperation");
            Check.NotNull(batchBuilder, "batchBuilder");

            batchBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(foreignKeyOperation.ForeignKeyName))
                .Append(" FOREIGN KEY (")
                .Append(foreignKeyOperation.ColumnNames.Select(DelimitIdentifier).Join())
                .Append(") REFERENCES ")
                .Append(DelimitIdentifier(foreignKeyOperation.ReferencedTableName))
                .Append(" (")
                .Append(foreignKeyOperation.ReferencedColumnNames.Select(DelimitIdentifier).Join())
                .Append(")");

            if (foreignKeyOperation.CascadeDelete)
            {
                batchBuilder.Append(" ON DELETE CASCADE");
            }
        }
    }
}
