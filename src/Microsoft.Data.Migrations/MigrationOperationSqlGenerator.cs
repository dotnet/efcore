// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using System.Globalization;

namespace Microsoft.Data.Migrations
{
    /// <summary>
    /// Default migration operation SQL generator, outputs best-effort ANSI-99 compliant SQL.
    /// </summary>
    // TODO: Include idempotent generation logic here. Can use the presence checks methods
    // similar to ones in the generator for SqlServer but implemented over INFORMATION_SCHEMA.
    // Also consider adding flag for opting between presence checks and "IF [NOT] EXISTS" 
    // constructs where possible.
    public class MigrationOperationSqlGenerator : MigrationOperationVisitor
    {
        // TODO: Check whether the following formats ar SqlServer specific or not and move
        // to SqlServer provider if they are.
        internal const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        internal const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private readonly IndentedStringBuilder _stringBuilder = new IndentedStringBuilder();
        private Database _database;

        public virtual string GeneratedSql
        {
            get { return StringBuilder.ToString();  }
        }

        public virtual Database Database 
        {
            get { return _database; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _database = value;
            }
        }

        protected IndentedStringBuilder StringBuilder
        {
            get { return _stringBuilder; }
        }

        public static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var sqlGenerator = new MigrationOperationSqlGenerator();

            migrationOperation.Accept(sqlGenerator);

            return sqlGenerator.GeneratedSql;
        }

        public virtual void Generate([NotNull] IReadOnlyList<MigrationOperation> migrationOperations)
        {
            Check.NotNull(migrationOperations, "migrationOperations");

            foreach (var operation in migrationOperations)
            {
                operation.Accept(this);
            }
        }

        public override void Visit([NotNull] CreateDatabaseOperation createDatabaseOperation)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");

            StringBuilder
                .Append("CREATE DATABASE ")
                .Append(DelimitIdentifier(createDatabaseOperation.DatabaseName));
        }

        public override void Visit([NotNull] DropDatabaseOperation dropDatabaseOperation)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");

            StringBuilder
                .Append("DROP DATABASE ")
                .Append(DelimitIdentifier(dropDatabaseOperation.DatabaseName));
        }

        public override void Visit([NotNull] CreateSequenceOperation createSequenceOperation)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");

            var sequence = createSequenceOperation.Sequence;

            StringBuilder
                .Append("CREATE SEQUENCE ")
                .Append(DelimitIdentifier(sequence.Name))
                .Append(" AS ")
                .Append(sequence.DataType)
                .Append(" START WITH ")
                .Append(sequence.StartWith)
                .Append(" INCREMENT BY ")
                .Append(sequence.IncrementBy);
        }

        public override void Visit([NotNull] DropSequenceOperation dropSequenceOperation)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");

            StringBuilder
                .Append("DROP SEQUENCE ")
                .Append(DelimitIdentifier(dropSequenceOperation.SequenceName));
        }

        public override void Visit([NotNull] CreateTableOperation createTableOperation)
        {
            Check.NotNull(createTableOperation, "createTableOperation");

            var table = createTableOperation.Table;

            StringBuilder
                .Append("CREATE TABLE ")
                .Append(DelimitIdentifier(table.Name))
                .AppendLine(" (");

            using (StringBuilder.Indent())
            {
                GenerateColumns(table.Columns);

                var primaryKey = table.PrimaryKey;

                if (primaryKey != null)
                {
                    StringBuilder.AppendLine();

                    GeneratePrimaryKey(
                        primaryKey.Name, 
                        primaryKey.Columns.Select(c => c.Name).ToArray(),
                        primaryKey.IsClustered);
                }
            }

            StringBuilder
                .AppendLine()
                .Append(")");
        }

        public override void Visit([NotNull] DropTableOperation dropTableOperation)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");

            StringBuilder
                .Append("DROP TABLE ")
                .Append(DelimitIdentifier(dropTableOperation.TableName));
        }

        public override void Visit([NotNull] RenameTableOperation renameTableOperation)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");

            // TODO: Not ANSI-99.

            StringBuilder
                .Append("EXECUTE sp_rename @objname = N")
                .Append(DelimitLiteral(renameTableOperation.TableName))
                .Append(", @newname = N")
                .Append(DelimitLiteral(renameTableOperation.NewTableName))
                .Append(", @objtype = N'OBJECT'");
        }

        public override void Visit([NotNull] MoveTableOperation moveTableOperation)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");

            StringBuilder
                .Append("ALTER SCHEMA ")
                .Append(DelimitIdentifier(moveTableOperation.NewSchema))
                .Append(" TRANSFER ")
                .Append(DelimitIdentifier(moveTableOperation.TableName));
        }

        public override void Visit([NotNull] AddColumnOperation addColumnOperation)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addColumnOperation.TableName))
                .Append(" ADD ");

            GenerateColumn(addColumnOperation.Column);
        }

        public override void Visit([NotNull] DropColumnOperation dropColumnOperation)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropColumnOperation.TableName))
                .Append(" DROP COLUMN ")
                .Append(DelimitIdentifier(dropColumnOperation.ColumnName));
        }

        public override void Visit([NotNull] AlterColumnOperation alterColumnOperation)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(alterColumnOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(alterColumnOperation.ColumnName))
                .Append(" ")
                .Append(alterColumnOperation.DataType)
                .Append(alterColumnOperation.IsNullable ? " NULL" : " NOT NULL");
        }

        public override void Visit([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(addDefaultConstraintOperation.ColumnName))
                .Append(" SET DEFAULT ");

            if (addDefaultConstraintOperation.DefaultSql != null)
            {
                StringBuilder.Append(addDefaultConstraintOperation.DefaultSql);
            }
            else 
            {
                StringBuilder.Append(GenerateLiteral((dynamic)addDefaultConstraintOperation.DefaultValue));
            }
        }

        public override void Visit([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.TableName))
                .Append(" ALTER COLUMN ")
                .Append(DelimitIdentifier(dropDefaultConstraintOperation.ColumnName))
                .Append(" DROP DEFAULT");
        }

        public override void Visit([NotNull] RenameColumnOperation renameColumnOperation)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");

            // TODO: Not ANSI-99.

            StringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameColumnOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameColumnOperation.ColumnName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameColumnOperation.NewColumnName))
                .Append(", @objtype = N'COLUMN'");
        }

        public override void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.TableName))
                .Append(" ADD ");

            GeneratePrimaryKey(
                addPrimaryKeyOperation.PrimaryKeyName,
                addPrimaryKeyOperation.ColumnNames,
                addPrimaryKeyOperation.IsClustered);
        }

        public override void Visit([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.TableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropPrimaryKeyOperation.PrimaryKeyName));
        }

        public override void Visit([NotNull] AddForeignKeyOperation addForeignKeyOperation)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");

            StringBuilder
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
                StringBuilder.Append(" ON DELETE CASCADE");
            }
        }

        public override void Visit([NotNull] DropForeignKeyOperation dropForeignKeyOperation)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.DependentTableName))
                .Append(" DROP CONSTRAINT ")
                .Append(DelimitIdentifier(dropForeignKeyOperation.ForeignKeyName));
        }

        public override void Visit([NotNull] CreateIndexOperation createIndexOperation)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");

            StringBuilder.Append("CREATE");

            if (createIndexOperation.IsUnique)
            {
                StringBuilder.Append(" UNIQUE");
            }

            if (createIndexOperation.IsClustered)
            {
                StringBuilder.Append(" CLUSTERED");
            }

            StringBuilder
                .Append(" INDEX ")
                .Append(DelimitIdentifier(createIndexOperation.IndexName))
                .Append(" ON ")
                .Append(DelimitIdentifier(createIndexOperation.TableName))
                .Append(" (")
                .Append(createIndexOperation.ColumnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(")");
        }

        public override void Visit([NotNull] DropIndexOperation dropIndexOperation)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");

            StringBuilder
                .Append("DROP INDEX ")
                .Append(DelimitIdentifier(dropIndexOperation.IndexName))
                .Append(" ON ")
                .Append(DelimitIdentifier(dropIndexOperation.TableName));
        }

        public override void Visit([NotNull] RenameIndexOperation renameIndexOperation)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");

            // TODO: Not ANSI-99.

            StringBuilder
                .Append("EXECUTE sp_rename @objname = N'")
                .Append(EscapeLiteral(renameIndexOperation.TableName))
                .Append(".")
                .Append(EscapeLiteral(renameIndexOperation.IndexName))
                .Append("', @newname = N")
                .Append(DelimitLiteral(renameIndexOperation.NewIndexName))
                .Append(", @objtype = N'INDEX'");
        }

        public virtual string GenerateDataType([NotNull] Type clrType)
        {
            Check.NotNull(clrType, "clrType");

            throw new NotImplementedException();
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

        protected virtual void GenerateColumns([NotNull] IReadOnlyList<Column> columns)
        {
            Check.NotNull(columns, "columns");

            if (columns.Count == 0)
            {
                return;
            }

            GenerateColumn(columns[0]);

            for (var i = 1; i < columns.Count; i++)
            {
                StringBuilder.AppendLine(",");

                GenerateColumn(columns[i]);
            }
        }

        protected virtual void GenerateColumn([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            StringBuilder
                .Append(DelimitIdentifier(column.Name))
                .Append(" ");

            if (column.DataType != null)
            {
                StringBuilder.Append(column.DataType);
            }
            else
            {
                StringBuilder.Append(GenerateDataType(column.ClrType));
            }

            if (!column.IsNullable)
            {
                StringBuilder.Append(" NOT NULL");
            }

            if (column.DefaultSql != null)
            {
                StringBuilder
                    .Append(" DEFAULT ")
                    .Append(column.DefaultSql);
            }
            else if (column.DefaultValue != null)
            {
                StringBuilder
                    .Append(" DEFAULT ")
                    .Append(GenerateLiteral(column.DefaultValue));
            }
        }

        protected virtual void GeneratePrimaryKey(
            [NotNull] string primaryKeyName,
            [NotNull] IReadOnlyList<string> columnNames,
            bool isClustered)
        {
            Check.NotNull(primaryKeyName, "primaryKeyName");
            Check.NotNull(columnNames, "columnNames");

            StringBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(primaryKeyName))
                .Append(" PRIMARY KEY");

            if (!isClustered)
            {
                StringBuilder.Append(" NONCLUSTERED");
            }

            StringBuilder
                .Append(" (")
                .Append(columnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(")");
        }
    }
}
