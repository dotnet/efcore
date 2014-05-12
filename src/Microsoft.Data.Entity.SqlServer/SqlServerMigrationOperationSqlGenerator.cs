// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        private int _variableCount;

        public SqlServerMigrationOperationSqlGenerator([NotNull] SqlServerTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public override void Generate(CreateDatabaseOperation createDatabaseOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateDatabasePresenceCheck(createDatabaseOperation.DatabaseName, negative: true, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(createDatabaseOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(createDatabaseOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropDatabaseOperation dropDatabaseOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateDatabasePresenceCheck(dropDatabaseOperation.DatabaseName, negative: false, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropDatabaseOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropDatabaseOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(CreateSequenceOperation createSequenceOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateSequencePresenceCheck(createSequenceOperation.Sequence.Name, negative: true, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(createSequenceOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(createSequenceOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropSequenceOperation dropSequenceOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateSequencePresenceCheck(dropSequenceOperation.SequenceName, negative: false, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropSequenceOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropSequenceOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(CreateTableOperation createTableOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateTablePresenceCheck(createTableOperation.Table.Name, negative: true, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(createTableOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(createTableOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropTableOperation dropTableOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateTablePresenceCheck(dropTableOperation.TableName, negative: false, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropTableOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropTableOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(RenameTableOperation renameTableOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateTablePresenceCheck(renameTableOperation.TableName, negative: false, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(renameTableOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(renameTableOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(MoveTableOperation moveTableOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateTablePresenceCheck(moveTableOperation.TableName, negative: false, builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(moveTableOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(moveTableOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(AddColumnOperation addColumnOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    addColumnOperation.TableName,
                    addColumnOperation.Column.Name,
                    negative: true,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(addColumnOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(addColumnOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropColumnOperation dropColumnOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    dropColumnOperation.TableName,
                    dropColumnOperation.ColumnName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropColumnOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropColumnOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(AlterColumnOperation alterColumnOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    alterColumnOperation.TableName,
                    alterColumnOperation.NewColumn.Name,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(alterColumnOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(alterColumnOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateColumnDefaultPresenceCheck(
                    addDefaultConstraintOperation.TableName,
                    addDefaultConstraintOperation.ColumnName,
                    negative: true,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    Generate(addDefaultConstraintOperation, stringBuilder);
                }
            }
            else
            {
                Generate(addDefaultConstraintOperation, stringBuilder);
            }
        }

        public override void Generate(DropDefaultConstraintOperation dropDefaultConstraintOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            var constraintNameVariable = "@var" + _variableCount++;

            stringBuilder
                .Append("DECLARE ")
                .Append(constraintNameVariable)
                .AppendLine(" nvarchar(128)");

            stringBuilder
                .Append("SELECT ")
                .Append(constraintNameVariable)
                .Append(" = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(dropDefaultConstraintOperation.TableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .AppendLine(DelimitLiteral(dropDefaultConstraintOperation.ColumnName));

            if (generateIdempotentSql)
            {
                stringBuilder
                    .Append("IF ")
                    .Append(constraintNameVariable)
                    .AppendLine(" IS NOT NULL");

                using (stringBuilder.Indent())
                {
                    GenerateExecuteDropConstraint(dropDefaultConstraintOperation.TableName, constraintNameVariable, stringBuilder);
                }
            }
            else
            {
                GenerateExecuteDropConstraint(dropDefaultConstraintOperation.TableName, constraintNameVariable, stringBuilder);
            }
        }

        public override void Generate(RenameColumnOperation renameColumnOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    renameColumnOperation.TableName,
                    renameColumnOperation.ColumnName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(renameColumnOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(renameColumnOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GeneratePrimaryKeyPresenceCheck(
                    addPrimaryKeyOperation.TableName,
                    addPrimaryKeyOperation.PrimaryKeyName,
                    negative: true,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(addPrimaryKeyOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(addPrimaryKeyOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GeneratePrimaryKeyPresenceCheck(
                    dropPrimaryKeyOperation.TableName,
                    dropPrimaryKeyOperation.PrimaryKeyName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropPrimaryKeyOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropPrimaryKeyOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(AddForeignKeyOperation addForeignKeyOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateForeignKeyPresenceCheck(
                    addForeignKeyOperation.TableName,
                    addForeignKeyOperation.ForeignKeyName,
                    negative: true,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(addForeignKeyOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(addForeignKeyOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropForeignKeyOperation dropForeignKeyOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateForeignKeyPresenceCheck(
                    dropForeignKeyOperation.TableName,
                    dropForeignKeyOperation.ForeignKeyName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropForeignKeyOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropForeignKeyOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(CreateIndexOperation createIndexOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    createIndexOperation.TableName,
                    createIndexOperation.IndexName,
                    negative: true,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(createIndexOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(createIndexOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(DropIndexOperation dropIndexOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    dropIndexOperation.TableName,
                    dropIndexOperation.IndexName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(dropIndexOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(dropIndexOperation, stringBuilder, generateIdempotentSql);
            }
        }

        public override void Generate(RenameIndexOperation renameIndexOperation, IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");
            Check.NotNull(stringBuilder, "stringBuilder");

            if (generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    renameIndexOperation.TableName,
                    renameIndexOperation.IndexName,
                    negative: false,
                    builder: stringBuilder);

                using (stringBuilder.AppendLine().Indent())
                {
                    base.Generate(renameIndexOperation, stringBuilder, generateIdempotentSql: false);
                }
            }
            else
            {
                base.Generate(renameIndexOperation, stringBuilder, generateIdempotentSql);
            }
        }

        protected internal virtual void GenerateDatabasePresenceCheck([NotNull] string databaseName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(databaseName, "databaseName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.databases WHERE name = N")
                .Append(DelimitLiteral(databaseName))
                .Append(")");
        }

        protected internal virtual void GenerateSequencePresenceCheck(SchemaQualifiedName sequenceName, bool negative, IndentedStringBuilder builder)
        {
            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.sequences WHERE name = N")
                .Append(DelimitLiteral(sequenceName.Name));

            if (sequenceName.IsSchemaQualified)
            {
                builder
                    .Append(" AND schema_id = SCHEMA_ID(N")
                    .Append(DelimitLiteral(sequenceName.Schema))
                    .Append(")");
            }

            builder.Append(")");
        }

        protected internal virtual void GenerateTablePresenceCheck(SchemaQualifiedName tableName, bool negative, IndentedStringBuilder builder)
        {
            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.tables WHERE name = N")
                .Append(DelimitLiteral(tableName.Name));

            if (tableName.IsSchemaQualified)
            {
                builder
                    .Append(" AND schema_id = SCHEMA_ID(N")
                    .Append(DelimitLiteral(tableName.Schema))
                    .Append(")");
            }

            builder.Append(")");
        }

        protected internal virtual void GenerateColumnPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string columnName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(columnName, "columnName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.columns WHERE name = N")
                .Append(DelimitLiteral(columnName))
                .Append(" AND object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        protected internal virtual void GeneratePrimaryKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string primaryKeyName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.key_constraints WHERE type = 'PK'");

            if (!negative)
            {
                builder
                    .Append(" AND name = N")
                    .Append(DelimitLiteral(primaryKeyName));
            }

            builder
                .Append(" AND parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(")");

            builder.Append(")");
        }

        protected internal virtual void GenerateForeignKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string foreignKeyName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N")
                .Append(DelimitLiteral(foreignKeyName))
                .Append(" AND parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        protected internal virtual void GenerateColumnDefaultPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string columnName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(columnName, "columnName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .Append(DelimitLiteral(columnName))
                .Append(")");
        }

        protected internal virtual void GenerateIndexPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string indexName, bool negative, IndentedStringBuilder builder)
        {
            Check.NotEmpty(indexName, "indexName");

            builder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.indexes WHERE name = N")
                .Append(DelimitLiteral(indexName))
                .Append(" AND object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        private void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation, IndentedStringBuilder builder)
        {
            var tableName = addDefaultConstraintOperation.TableName;
            var columnName = addDefaultConstraintOperation.ColumnName;

            builder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier("DF_" + tableName + "_" + columnName))
                .Append(" DEFAULT ");

            if (addDefaultConstraintOperation.DefaultSql != null)
            {
                builder.Append(addDefaultConstraintOperation.DefaultSql);
            }
            else
            {
                builder.Append(GenerateLiteral(addDefaultConstraintOperation.DefaultValue));
            }

            builder
                .Append(" FOR ")
                .Append(DelimitIdentifier(columnName));
        }

        private void GenerateExecuteDropConstraint(
            SchemaQualifiedName tableName, [NotNull] string constraintNameVariable, IndentedStringBuilder builder)
        {
            builder
                .Append("EXECUTE('ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" DROP CONSTRAINT \"' + ")
                .Append(constraintNameVariable)
                .Append(" + '\"')");
        }

        public override string DelimitIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return "[" + EscapeIdentifier(identifier) + "]";
        }

        public override string EscapeIdentifier(string identifier)
        {
            Check.NotEmpty(identifier, "identifier");

            return identifier.Replace("]", "]]");
        }
    }
}
