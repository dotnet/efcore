// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        private readonly bool _generateIdempotentSql;
        private int _variableCount;

        public SqlServerMigrationOperationSqlGenerator()
        {
            _generateIdempotentSql = false;
        }

        public SqlServerMigrationOperationSqlGenerator(bool generateIdempotentSql)
        {
            _generateIdempotentSql = generateIdempotentSql;
        }

        public new static string Generate<T>([NotNull] T migrationOperation)
            where T : MigrationOperation
        {
            Check.NotNull(migrationOperation, "migrationOperation");

            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator(generateIdempotentSql: true);

            migrationOperation.Accept(sqlGenerator);

            return sqlGenerator.GeneratedSql;
        }

        public override void Visit([NotNull] CreateDatabaseOperation createDatabaseOperation)
        {
            Check.NotNull(createDatabaseOperation, "createDatabaseOperation");

            if (_generateIdempotentSql)
            {
                GenerateDatabasePresenceCheck(createDatabaseOperation.DatabaseName, negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(createDatabaseOperation);
                }
            }
            else
            {
                base.Visit(createDatabaseOperation);
            }
        }

        public override void Visit([NotNull] DropDatabaseOperation dropDatabaseOperation)
        {
            Check.NotNull(dropDatabaseOperation, "dropDatabaseOperation");

            if (_generateIdempotentSql)
            {
                GenerateDatabasePresenceCheck(dropDatabaseOperation.DatabaseName, negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropDatabaseOperation);
                }
            }
            else
            {
                base.Visit(dropDatabaseOperation);
            }
        }

        public override void Visit([NotNull] CreateSequenceOperation createSequenceOperation)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");

            if (_generateIdempotentSql)
            {
                GenerateSequencePresenceCheck(createSequenceOperation.Sequence.Name, negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(createSequenceOperation);
                }
            }
            else
            {
                base.Visit(createSequenceOperation);
            }
        }

        public override void Visit([NotNull] DropSequenceOperation dropSequenceOperation)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");

            if (_generateIdempotentSql)
            {
                GenerateSequencePresenceCheck(dropSequenceOperation.SequenceName, negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropSequenceOperation);
                }
            }
            else
            {
                base.Visit(dropSequenceOperation);
            }
        }

        public override void Visit([NotNull] CreateTableOperation createTableOperation)
        {
            Check.NotNull(createTableOperation, "createTableOperation");

            if (_generateIdempotentSql)
            {
                GenerateTablePresenceCheck(createTableOperation.Table.Name, negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(createTableOperation);
                }
            }
            else
            {
                base.Visit(createTableOperation);
            }
        }

        public override void Visit([NotNull] DropTableOperation dropTableOperation)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");

            if (_generateIdempotentSql)
            {
                GenerateTablePresenceCheck(dropTableOperation.TableName, negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropTableOperation);
                }
            }
            else
            {
                base.Visit(dropTableOperation);
            }
        }

        public override void Visit([NotNull] RenameTableOperation renameTableOperation)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");

            if (_generateIdempotentSql)
            {
                GenerateTablePresenceCheck(renameTableOperation.TableName, negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(renameTableOperation);
                }
            }
            else
            {
                base.Visit(renameTableOperation);
            }
        }

        public override void Visit([NotNull] MoveTableOperation moveTableOperation)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");

            if (_generateIdempotentSql)
            {
                GenerateTablePresenceCheck(moveTableOperation.TableName, negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(moveTableOperation);
                }
            }
            else
            {
                base.Visit(moveTableOperation);
            }
        }

        public override void Visit([NotNull] AddColumnOperation addColumnOperation)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    addColumnOperation.TableName,
                    addColumnOperation.Column.Name,
                    negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(addColumnOperation);
                }
            }
            else
            {
                base.Visit(addColumnOperation);
            }
        }

        public override void Visit([NotNull] DropColumnOperation dropColumnOperation)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    dropColumnOperation.TableName,
                    dropColumnOperation.ColumnName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropColumnOperation);
                }
            }
            else
            {
                base.Visit(dropColumnOperation);
            }
        }

        public override void Visit([NotNull] AlterColumnOperation alterColumnOperation)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    alterColumnOperation.TableName,
                    alterColumnOperation.ColumnName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(alterColumnOperation);
                }
            }
            else
            {
                base.Visit(alterColumnOperation);
            }
        }

        public override void Visit([NotNull] AddDefaultConstraintOperation addDefaultConstraintOperation)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnDefaultPresenceCheck(
                    addDefaultConstraintOperation.TableName,
                    addDefaultConstraintOperation.ColumnName,
                    negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    Generate(addDefaultConstraintOperation);
                }
            }
            else
            {
                Generate(addDefaultConstraintOperation);
            }
        }

        public override void Visit([NotNull] DropDefaultConstraintOperation dropDefaultConstraintOperation)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");

            var constraintNameVariable = "@var" + _variableCount++;

            StringBuilder
                .Append("DECLARE ")
                .Append(constraintNameVariable)
                .AppendLine(" nvarchar(128)");

            StringBuilder
                .Append("SELECT ")
                .Append(constraintNameVariable)
                .Append(" = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(dropDefaultConstraintOperation.TableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .AppendLine(DelimitLiteral(dropDefaultConstraintOperation.ColumnName));

            if (_generateIdempotentSql)
            {
                StringBuilder
                    .Append("IF ")
                    .Append(constraintNameVariable)
                    .AppendLine(" IS NOT NULL");

                using (StringBuilder.Indent())
                {
                    GenerateExecuteDropConstraint(dropDefaultConstraintOperation.TableName, constraintNameVariable);
                }
            }
            else
            {
                GenerateExecuteDropConstraint(dropDefaultConstraintOperation.TableName, constraintNameVariable);
            }
        }

        public override void Visit([NotNull] RenameColumnOperation renameColumnOperation)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    renameColumnOperation.TableName,
                    renameColumnOperation.ColumnName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(renameColumnOperation);
                }
            }
            else
            {
                base.Visit(renameColumnOperation);
            }
        }

        public override void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            if (_generateIdempotentSql)
            {
                GeneratePrimaryKeyPresenceCheck(
                    addPrimaryKeyOperation.TableName,
                    addPrimaryKeyOperation.PrimaryKeyName,
                    negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(addPrimaryKeyOperation);
                }
            }
            else
            {
                base.Visit(addPrimaryKeyOperation);
            }
        }

        public override void Visit([NotNull] DropPrimaryKeyOperation dropPrimaryKeyOperation)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");

            if (_generateIdempotentSql)
            {
                GeneratePrimaryKeyPresenceCheck(
                    dropPrimaryKeyOperation.TableName,
                    dropPrimaryKeyOperation.PrimaryKeyName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropPrimaryKeyOperation);
                }
            }
            else
            {
                base.Visit(dropPrimaryKeyOperation);
            }
        }

        public override void Visit([NotNull] AddForeignKeyOperation addForeignKeyOperation)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");

            if (_generateIdempotentSql)
            {
                GenerateForeignKeyPresenceCheck(
                    addForeignKeyOperation.TableName,
                    addForeignKeyOperation.ForeignKeyName,
                    negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(addForeignKeyOperation);
                }
            }
            else
            {
                base.Visit(addForeignKeyOperation);
            }
        }

        public override void Visit([NotNull] DropForeignKeyOperation dropForeignKeyOperation)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");

            if (_generateIdempotentSql)
            {
                GenerateForeignKeyPresenceCheck(
                    dropForeignKeyOperation.DependentTableName,
                    dropForeignKeyOperation.ForeignKeyName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropForeignKeyOperation);
                }
            }
            else
            {
                base.Visit(dropForeignKeyOperation);
            }
        }

        public override void Visit([NotNull] CreateIndexOperation createIndexOperation)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");

            if (_generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    createIndexOperation.TableName,
                    createIndexOperation.IndexName,
                    negative: true);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(createIndexOperation);
                }
            }
            else
            {
                base.Visit(createIndexOperation);
            }
        }

        public override void Visit([NotNull] DropIndexOperation dropIndexOperation)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");

            if (_generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    dropIndexOperation.TableName,
                    dropIndexOperation.IndexName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(dropIndexOperation);
                }
            }
            else
            {
                base.Visit(dropIndexOperation);
            }
        }

        public override void Visit([NotNull] RenameIndexOperation renameIndexOperation)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");

            if (_generateIdempotentSql)
            {
                GenerateIndexPresenceCheck(
                    renameIndexOperation.TableName,
                    renameIndexOperation.IndexName,
                    negative: false);

                using (StringBuilder.AppendLine().Indent())
                {
                    base.Visit(renameIndexOperation);
                }
            }
            else
            {
                base.Visit(renameIndexOperation);
            }
        }

        protected internal virtual void GenerateDatabasePresenceCheck([NotNull] string databaseName, bool negative)
        {
            Check.NotEmpty(databaseName, "databaseName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.databases WHERE name = N")
                .Append(DelimitLiteral(databaseName))
                .Append(")");
        }

        protected internal virtual void GenerateSequencePresenceCheck(SchemaQualifiedName sequenceName, bool negative)
        {
            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.sequences WHERE name = N")
                .Append(DelimitLiteral(sequenceName.Name));

            if (sequenceName.IsSchemaQualified)
            {
                StringBuilder
                    .Append(" AND schema_id = SCHEMA_ID(N")
                    .Append(DelimitLiteral(sequenceName.Schema))
                    .Append(")");
            }

            StringBuilder.Append(")");
        }

        protected internal virtual void GenerateTablePresenceCheck(SchemaQualifiedName tableName, bool negative)
        {
            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.tables WHERE name = N")
                .Append(DelimitLiteral(tableName.Name));

            if (tableName.IsSchemaQualified)
            {
                StringBuilder
                    .Append(" AND schema_id = SCHEMA_ID(N")
                    .Append(DelimitLiteral(tableName.Schema))
                    .Append(")");
            }

            StringBuilder.Append(")");
        }

        protected internal virtual void GenerateColumnPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string columnName, bool negative)
        {
            Check.NotEmpty(columnName, "columnName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.columns WHERE name = N")
                .Append(DelimitLiteral(columnName))
                .Append(" AND object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        protected internal virtual void GeneratePrimaryKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string primaryKeyName, bool negative)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.key_constraints WHERE type = 'PK'");

            if (!negative)
            {
                StringBuilder
                    .Append(" AND name = N")
                    .Append(DelimitLiteral(primaryKeyName));
            }

            StringBuilder
                .Append(" AND parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(")");

            StringBuilder.Append(")");
        }

        protected internal virtual void GenerateForeignKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string foreignKeyName, bool negative)
        {
            Check.NotEmpty(foreignKeyName, "foreignKeyName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N")
                .Append(DelimitLiteral(foreignKeyName))
                .Append(" AND parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        protected internal virtual void GenerateColumnDefaultPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string columnName, bool negative)
        {
            Check.NotEmpty(columnName, "columnName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(") AND COL_NAME(parent_object_id, parent_column_id) = N")
                .Append(DelimitLiteral(columnName))
                .Append(")");
        }

        protected internal virtual void GenerateIndexPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string indexName, bool negative)
        {
            Check.NotEmpty(indexName, "indexName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.indexes WHERE name = N")
                .Append(DelimitLiteral(indexName))
                .Append(" AND object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        private void Generate(AddDefaultConstraintOperation addDefaultConstraintOperation)
        {
            var tableName = addDefaultConstraintOperation.TableName;
            var columnName = addDefaultConstraintOperation.ColumnName;

            StringBuilder
                .Append("ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" ADD CONSTRAINT ")
                .Append(DelimitIdentifier("DF_" + tableName + "_" + columnName))
                .Append(" DEFAULT ");

            if (addDefaultConstraintOperation.DefaultSql != null)
            {
                StringBuilder.Append(addDefaultConstraintOperation.DefaultSql);
            }
            else
            {
                StringBuilder.Append(GenerateLiteral(addDefaultConstraintOperation.DefaultValue));
            }

            StringBuilder
                .Append(" FOR ")
                .Append(DelimitIdentifier(columnName));
        }

        private void GenerateExecuteDropConstraint(
            SchemaQualifiedName tableName, [NotNull] string constraintNameVariable)
        {
            StringBuilder
                .Append("EXECUTE('ALTER TABLE ")
                .Append(DelimitIdentifier(tableName))
                .Append(" DROP CONSTRAINT \"' + ")
                .Append(constraintNameVariable)
                .Append(" + '\"')");
        }
    }
}
