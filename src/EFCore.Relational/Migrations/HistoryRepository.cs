// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    // TODO: Leverage query pipeline for GetAppliedMigrations
    // TODO: Leverage update pipeline for GetInsertScript & GetDeleteScript
    public abstract class HistoryRepository : IHistoryRepository
    {
        public const string DefaultTableName = "__EFMigrationsHistory";

        private readonly LazyRef<IModel> _model;
        private readonly LazyRef<string> _migrationIdColumnName;
        private readonly LazyRef<string> _productVersionColumnName;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected HistoryRepository([NotNull] HistoryRepositoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;

            var relationalOptions = RelationalOptionsExtension.Extract(dependencies.Options);
            TableName = relationalOptions?.MigrationsHistoryTableName ?? DefaultTableName;
            TableSchema = relationalOptions?.MigrationsHistoryTableSchema;
            _model = new LazyRef<IModel>(
                () =>
                    {
                        var modelBuilder = new ModelBuilder(new ConventionSet());
                        modelBuilder.Entity<HistoryRow>(
                            x =>
                                {
                                    ConfigureTable(x);
                                    x.ToTable(TableName, TableSchema);
                                });

                        return modelBuilder.Model;
                    });
            var entityType = new LazyRef<IEntityType>(() => _model.Value.FindEntityType(typeof(HistoryRow)));
            _migrationIdColumnName = new LazyRef<string>(
                () => entityType.Value.FindProperty(nameof(HistoryRow.MigrationId)).Relational().ColumnName);
            _productVersionColumnName = new LazyRef<string>(
                () => entityType.Value.FindProperty(nameof(HistoryRow.ProductVersion)).Relational().ColumnName);
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual HistoryRepositoryDependencies Dependencies { get; }

        protected virtual ISqlGenerationHelper SqlGenerationHelper => Dependencies.SqlGenerationHelper;

        protected virtual string TableName { get; }
        protected virtual string TableSchema { get; }
        protected virtual string MigrationIdColumnName => _migrationIdColumnName.Value;
        protected virtual string ProductVersionColumnName => _productVersionColumnName.Value;

        protected abstract string ExistsSql { get; }

        public virtual bool Exists()
            => Dependencies.DatabaseCreator.Exists()
               && InterpretExistsResult(
                   Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalar(Dependencies.Connection));

        public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await Dependencies.DatabaseCreator.ExistsAsync(cancellationToken)
               && InterpretExistsResult(
                   await Dependencies.RawSqlCommandBuilder.Build(ExistsSql).ExecuteScalarAsync(
                       Dependencies.Connection, cancellationToken: cancellationToken));

        /// <summary>
        ///     Interprets the result of executing <see cref="ExistsSql" />.
        /// </summary>
        /// <returns>true if the table exists; otherwise, false.</returns>
        protected abstract bool InterpretExistsResult([NotNull] object value);

        public abstract string GetCreateIfNotExistsScript();

        public virtual string GetCreateScript()
        {
            var operations = Dependencies.ModelDiffer.GetDifferences(null, _model.Value);
            var commandList = Dependencies.MigrationsSqlGenerator.Generate(operations, _model.Value);

            return string.Concat(commandList.Select(c => c.CommandText));
        }

        protected virtual void ConfigureTable([NotNull] EntityTypeBuilder<HistoryRow> history)
        {
            history.ToTable(DefaultTableName);
            history.HasKey(h => h.MigrationId);
            history.Property(h => h.MigrationId).HasMaxLength(150);
            history.Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
        }

        public virtual IReadOnlyList<HistoryRow> GetAppliedMigrations()
        {
            var rows = new List<HistoryRow>();

            if (Exists())
            {
                var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedMigrationsSql);

                using (var reader = command.ExecuteReader(Dependencies.Connection))
                {
                    while (reader.Read())
                    {
                        rows.Add(new HistoryRow(reader.DbDataReader.GetString(0), reader.DbDataReader.GetString(1)));
                    }
                }
            }

            return rows;
        }

        public virtual async Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var rows = new List<HistoryRow>();

            if (await ExistsAsync(cancellationToken))
            {
                var command = Dependencies.RawSqlCommandBuilder.Build(GetAppliedMigrationsSql);

                using (var reader = await command.ExecuteReaderAsync(Dependencies.Connection, cancellationToken: cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        rows.Add(new HistoryRow(reader.DbDataReader.GetString(0), reader.DbDataReader.GetString(1)));
                    }
                }
            }

            return rows;
        }

        protected virtual string GetAppliedMigrationsSql
            => new StringBuilder()
                .Append("SELECT ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .AppendLine(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
                .Append("FROM ")
                .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append("ORDER BY ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(SqlGenerationHelper.StatementTerminator)
                .ToString();

        public virtual string GetInsertScript(HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES ('")
                .Append(SqlGenerationHelper.EscapeLiteral(row.MigrationId))
                .Append("', '")
                .Append(SqlGenerationHelper.EscapeLiteral(row.ProductVersion))
                .Append("')")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .ToString();
        }

        public virtual string GetDeleteScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(SqlGenerationHelper.EscapeLiteral(migrationId))
                .Append("'")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .ToString();
        }

        public abstract string GetBeginIfNotExistsScript(string migrationId);
        public abstract string GetBeginIfExistsScript(string migrationId);
        public abstract string GetEndIfScript();
    }
}
