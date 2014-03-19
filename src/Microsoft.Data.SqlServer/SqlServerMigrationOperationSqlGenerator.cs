// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerMigrationOperationSqlGenerator : MigrationOperationSqlGenerator
    {
        private readonly bool _generateIdempotentSql;

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

        public override void Visit([NotNull] AlterColumnOperation alterColumnOperation)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");

            if (_generateIdempotentSql)
            {
                GenerateColumnPresenceCheck(
                    alterColumnOperation.TableName,
                    alterColumnOperation.Column.Name,
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

        public override void Visit([NotNull] AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");

            if (_generateIdempotentSql)
            {
                GeneratePrimaryKeyPresenceCheck(
                    addPrimaryKeyOperation.TableName,
                    addPrimaryKeyOperation.PrimaryKey.Name.Name, 
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
                    addForeignKeyOperation.DependentTableName,
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

        protected virtual void GenerateDatabasePresenceCheck([NotNull] string databaseName, bool negative)
        {
            Check.NotNull(databaseName, "databaseName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.databases WHERE name = ")
                .Append(DelimitLiteral(databaseName))
                .Append(")");
        }

        protected virtual void GenerateSequencePresenceCheck(SchemaQualifiedName sequenceName, bool negative)
        {
            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.sequences WHERE name = ")
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

        protected virtual void GenerateTablePresenceCheck(SchemaQualifiedName tableName, bool negative)
        {
            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.tables WHERE name = ")
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

        protected virtual void GenerateColumnPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string columnName, bool negative)
        {
            Check.NotNull(columnName, "columnName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.columns WHERE name = ")
                .Append(DelimitLiteral(columnName))
                .Append(" AND object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append("))");
        }

        protected virtual void GeneratePrimaryKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string primaryKeyName, bool negative)
        {
            Check.NotNull(primaryKeyName, "primaryKeyName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(")");

            if (!negative)
            {
                StringBuilder
                    .Append(" AND name = ")
                    .Append(DelimitLiteral(primaryKeyName));
            }

            StringBuilder.Append(")");
        }

        protected virtual void GenerateForeignKeyPresenceCheck(
            SchemaQualifiedName tableName, [NotNull] string foreignKeyName, bool negative)
        {
            Check.NotNull(foreignKeyName, "foreignKeyName");

            StringBuilder
                .Append("IF")
                .Append(negative ? " NOT" : string.Empty)
                .Append(" EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N")
                .Append(DelimitLiteral(tableName))
                .Append(")")
                .Append(" AND name = ")
                .Append(DelimitLiteral(foreignKeyName))
                .Append(")");
        }
    }
}
