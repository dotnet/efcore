// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    /// <summary>
    ///     Default migration operation SQL generator, outputs best-effort ANSI-99 compliant SQL.
    /// </summary>
    // TODO: Include idempotent generation logic here. Can use the presence checks methods
    // similar to ones in the generator for SqlServer but implemented over INFORMATION_SCHEMA.
    // Also consider adding flag for opting between presence checks and "IF [NOT] EXISTS" 
    // constructs where possible.
    public class MigrationOperationSqlGenerator
    {
        // TODO: Check whether the following formats ar SqlServer specific or not and move
        // to SqlServer provider if they are.
        internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        internal const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private DatabaseModel _database;
        private readonly RelationalTypeMapper _typeMapper;

        public MigrationOperationSqlGenerator([NotNull] RelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, "typeMapper");

            _typeMapper = typeMapper;
        }

        public virtual DatabaseModel Database
        {
            get { return _database; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _database = value;
            }
        }

        public virtual IEnumerable<SqlStatement> Generate([NotNull] IReadOnlyList<MigrationOperation> migrationOperations, bool generateIdempotentSql)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            foreach (var operation in migrationOperations)
            {
                var builder = new IndentedStringBuilder();
                operation.GenerateSql(this, builder, generateIdempotentSql);
                yield return new SqlStatement(builder.ToString());
            }
        }

        public virtual void Generate([NotNull] CreateDatabaseOperation createDatabaseOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CREATE DATABASE ")
                .Append(DelimitIdentifier(createDatabaseOperation.DatabaseName));
        }

        public virtual void Generate([NotNull] DropDatabaseOperation dropDatabaseOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");

            stringBuilder
                .Append("DROP DATABASE ")
                .Append(DelimitIdentifier(dropDatabaseOperation.DatabaseName));
        }

        public virtual void Generate([NotNull] CreateSequenceOperation createSequenceOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");

            var sequence = createSequenceOperation.Sequence;

            stringBuilder
                .Append("CREATE SEQUENCE ")
                .Append(DelimitIdentifier(sequence.Name))
                .Append(" AS ")
                .Append(sequence.DataType)
                .Append(" START WITH ")
                .Append(sequence.StartWith)
                .Append(" INCREMENT BY ")
                .Append(sequence.IncrementBy);
        }

        public virtual void Generate([NotNull] DropSequenceOperation dropSequenceOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");

            stringBuilder
                .Append("DROP SEQUENCE ")
                .Append(DelimitIdentifier(dropSequenceOperation.SequenceName));
        }

        public virtual void Generate([NotNull] CreateTableOperation createTableOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createTableOperation, "createTableOperation");

            var table = createTableOperation.Table;

            stringBuilder
                .Append("CREATE TABLE ")
                .Append(DelimitIdentifier(table.Name))
                .AppendLine(" (");

            using (stringBuilder.Indent())
            {
                GenerateColumns(table.Columns, stringBuilder);

                var primaryKey = table.PrimaryKey;

                if (primaryKey != null)
                {
                    stringBuilder.AppendLine(",");

                    GeneratePrimaryKey(
                        primaryKey.Name,
                        primaryKey.Columns.Select(c => c.Name).ToArray(),
                        primaryKey.IsClustered,
                        stringBuilder);
                }
            }

            stringBuilder
                .AppendLine()
                .Append(")");
        }

        public virtual void Generate([NotNull] DropTableOperation dropTableOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");

            stringBuilder
                .Append("DROP TABLE ")
                .Append(DelimitIdentifier(dropTableOperation.TableName));
        }

        public virtual void Generate([NotNull] RenameTableOperation renameTableOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");

            // TODO: Not ANSI-99.

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N")
                .Append(DelimitLiteral(renameTableOperation.TableName))
                .Append(", @newname = N")
                .Append(DelimitLiteral(renameTableOperation.NewTableName))
                .Append(", @objtype = N'OBJECT'");
        }

        public virtual void Generate([NotNull] MoveTableOperation moveTableOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");

            stringBuilder
                .Append("ALTER SCHEMA ")
                .Append(DelimitIdentifier(moveTableOperation.NewSchema))
                .Append(" TRANSFER ")
                .Append(DelimitIdentifier(moveTableOperation.TableName));
        }

        public virtual void Generate([NotNull] AddColumnOperation addColumnOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addColumnOperation.TableName))
                .Append(" ADD ");

            GenerateColumn(addColumnOperation.Column, stringBuilder);
        }

        public virtual void Generate([NotNull] DropColumnOperation dropColumnOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropColumnOperation.TableName))
                .Append(" DROP COLUMN ")
                .Append(DelimitIdentifier(dropColumnOperation.ColumnName));
        }

        public virtual void Generate([NotNull] AlterColumnOperation alterColumnOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

            var newColumn = alterColumnOperation.NewColumn;

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(alterColumnOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(newColumn.Name))
                .Append(" ")
                .Append(GenerateDataType(newColumn))
                .Append(newColumn.IsNullable ? " NULL" : " NOT NULL");
        }

        public virtual void Generate([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.ColumnName))
                .Append(" SET DEFAULT ");

            if (addDefaultConstraintOperation.DefaultSql != null)
            {
                stringBuilder.Append(addDefaultConstraintOperation.DefaultSql);
            }
            else
            {
                stringBuilder.Append(GenerateLiteral((dynamic)addDefaultConstraintOperation.DefaultValue));
            }
        }

        public virtual void Generate([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.ColumnName))
                .Append(" DROP DEFAULT");
        }

        public virtual void Generate([NotNull] RenameColumnOperation renameColumnOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");

            // TODO: Not ANSI-99.

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameColumnOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameColumnOperation.ColumnName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameColumnOperation.NewColumnName))
                .Append(", @objtype = N'COLUMN'")
                .ToString();
        }

        public virtual void Generate([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.TableName))
                .Append(" ADD ");

            GeneratePrimaryKey(
                addPrimaryKeyOperation.PrimaryKeyName,
                addPrimaryKeyOperation.ColumnNames,
                addPrimaryKeyOperation.IsClustered,
                stringBuilder);
        }

        public virtual void Generate([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.PrimaryKeyName));
        }

        public virtual void Generate([NotNull] AddForeignKeyOperation addForeignKeyOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addForeignKeyOperation.TableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier(addForeignKeyOperation.ForeignKeyName))
                .Append(" FOREIGN KEY (")
                .Append(addForeignKeyOperation.ColumnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(") REFERENCES ")
                .Append(DelimitIdentifier(addForeignKeyOperation.ReferencedTableName))
                .Append(" (")
                .Append(addForeignKeyOperation.ReferencedColumnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(")");

            if (addForeignKeyOperation.CascadeDelete)
            {
                stringBuilder.Append(" ON DELETE CASCADE");
            }
        }

        public virtual void Generate([NotNull] DropForeignKeyOperation dropForeignKeyOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");

            stringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.ForeignKeyName));
        }

        public virtual void Generate([NotNull] CreateIndexOperation createIndexOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");

            stringBuilder.Append("CREATE");

            if (createIndexOperation.IsUnique)
            {
                stringBuilder.Append(" UNIQUE");
            }

            if (createIndexOperation.IsClustered)
            {
                stringBuilder.Append(" CLUSTERED");
            }

            stringBuilder
                .Append(" INDEX ")
                .Append(DelimitIdentifier(createIndexOperation.IndexName))
                .Append(" ON ")
                .Append(DelimitIdentifier(createIndexOperation.TableName))
                .Append(" (")
                .Append(createIndexOperation.ColumnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(")");
        }

        public virtual void Generate([NotNull] DropIndexOperation dropIndexOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");

            stringBuilder
                .Append("DROP INDEX ")
                .Append(DelimitIdentifier(dropIndexOperation.IndexName))
                .Append(" ON ")
                .Append(DelimitIdentifier(dropIndexOperation.TableName));
        }

        public virtual void Generate([NotNull] RenameIndexOperation renameIndexOperation, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");

            // TODO: Not ANSI-99.

            stringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameIndexOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameIndexOperation.IndexName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameIndexOperation.NewIndexName))
                .Append(", @objtype = N'INDEX'");
        }

        public virtual string GenerateDataType([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            if (!string.IsNullOrEmpty(column.DataType))
            {
                return column.DataType;
            }

            var isKey = column.Table.PrimaryKey != null
                        && column.Table.PrimaryKey.Columns.Contains(column);

            return _typeMapper.GetTypeMapping(column.DataType, column.Name, column.ClrType, isKey, column.IsTimestamp).StoreTypeName;
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

            return "'" + value + "'";
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

        public virtual string DelimitLiteral([NotNull] string literal)
        {
            Check.NotNull(literal, "literal");

            return "'" + EscapeLiteral(literal) + "'";
        }

        public virtual string EscapeLiteral([NotNull] string literal)
        {
            Check.NotNull(literal, "literal");

            return literal.Replace("'", "''");
        }

        protected virtual void GenerateColumns([NotNull] IReadOnlyList<Column> columns, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(columns, "columns");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (columns.Count == 0)
            {
                return;
            }

            GenerateColumn(columns[0], stringBuilder);

            for (var i = 1; i < columns.Count; i++)
            {
                stringBuilder.AppendLine(",");

                GenerateColumn(columns[i], stringBuilder);
            }
        }

        protected virtual void GenerateColumn([NotNull] Column column, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(column, "column");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append(DelimitIdentifier(column.Name))
                .Append(" ");

            if (column.DataType != null)
            {
                stringBuilder.Append(column.DataType);
            }
            else
            {
                stringBuilder.Append(GenerateDataType(column));
            }

            if (!column.IsNullable)
            {
                stringBuilder.Append(" NOT NULL");
            }

            // TODO: Move to SQL Server specific generation
            if (column.ValueGenerationStrategy == StoreValueGenerationStrategy.Identity)
            {
                stringBuilder.Append(" IDENTITY");
            }

            if (column.DefaultSql != null)
            {
                stringBuilder
                    .Append(" DEFAULT ")
                    .Append(column.DefaultSql);
            }
            else if (column.DefaultValue != null)
            {
                stringBuilder
                    .Append(" DEFAULT ")
                    .Append(GenerateLiteral(column.DefaultValue));
            }
        }

        protected virtual void GeneratePrimaryKey(
            [NotNull] string primaryKeyName,
            [NotNull] IReadOnlyList<string> columnNames,
            bool isClustered,
            [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(primaryKeyName, "primaryKeyName");
            Check.NotNull(columnNames, "columnNames");
            Check.NotNull(stringBuilder, "stringBuilder");

            stringBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(primaryKeyName))
                .Append(" PRIMARY KEY");

            // TODO: Not ANSI-99
            if (!isClustered)
            {
                stringBuilder.Append(" NONCLUSTERED");
            }

            stringBuilder
                .Append(" (")
                .Append(columnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(")");
        }
    }
}
