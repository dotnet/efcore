// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Migrations
{
    /// <summary>
    /// Default migration operation SQL generator, outputs best-effort ANSI-99 compliant SQL.
    /// </summary>
    public class MigrationOperationSqlGenerator : MigrationOperationVisitor
    {
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
                Generate(table.Columns);

                if (table.PrimaryKey != null)
                {
                    StringBuilder.AppendLine();

                    Generate(table.PrimaryKey);
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

            Generate(addColumnOperation.Column);
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

        public override void Visit([NotNull] AlterColumnOperation alterColumnOperation)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

            // TODO: Not implemented.
        }

        public override void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(addPrimaryKeyOperation.TableName))
                .Append(" ADD ");

            Generate(addPrimaryKeyOperation.PrimaryKey);
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
                .Append(DelimitIdentifier(addForeignKeyOperation.DependentTableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier(addForeignKeyOperation.ForeignKeyName))
                .Append(" FOREIGN KEY (")
                .Append(addForeignKeyOperation.DependentColumnNames.Select(n => DelimitIdentifier(n)).Join())
                .Append(") REFERENCES ")
                .Append(DelimitIdentifier(addForeignKeyOperation.PrincipalTableName))
                .Append(" (")
                .Append(addForeignKeyOperation.PrincipalColumnNames.Select(n => DelimitIdentifier(n)).Join())
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

        internal virtual void Generate([NotNull] IReadOnlyList<Column> columns)
        {
            Check.NotNull(columns, "columns");
            
            // TODO: Table must enforce having at least one column.

            Generate(columns[0]);

            for (var i = 1; i < columns.Count; i++)
            {
                StringBuilder.AppendLine(",");

                Generate(columns[i]);
            }
        }

        internal protected virtual void Generate([NotNull] Column column)
        {
            Check.NotNull(column, "column");

            StringBuilder
                .Append(DelimitIdentifier(column.Name))
                .Append(" ")
                .Append(column.DataType);

            if (!column.IsNullable)
            {
                StringBuilder.Append(" NOT NULL");
            }

            if (!string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                StringBuilder
                    .Append(" DEFAULT ")
                    .Append(column.DefaultValue);
            }
        }

        internal protected virtual void Generate([NotNull] PrimaryKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            StringBuilder
                .Append("CONSTRAINT ")
                .Append(DelimitIdentifier(primaryKey.Name.Name))
                .Append(" PRIMARY KEY");

            if (!primaryKey.IsClustered)
            {
                StringBuilder.Append(" NONCLUSTERED");
            }

            StringBuilder
                .Append(" (")
                .Append(primaryKey.Columns.Select(c => DelimitIdentifier(c.Name)).Join())
                .Append(")");
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
    }
}
