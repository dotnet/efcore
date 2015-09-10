// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

namespace Microsoft.Data.Entity.Migrations
{
    // TODO: Leverage query pipeline for GetAppliedMigrations
    // TODO: Leverage update pipeline for GetInsertScript & GetDeleteScript
    public abstract class HistoryRepository : IHistoryRepository
    {
        public const string DefaultTableName = "__EFMigrationsHistory";

        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly ISqlStatementExecutor _executor;
        private readonly IRelationalConnection _connection;
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly LazyRef<IModel> _model;
        private readonly LazyRef<string> _migrationIdColumnName;
        private readonly LazyRef<string> _productVersionColumnName;

        public HistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlStatementExecutor executor,
            [NotNull] IRelationalConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalMetadataExtensionProvider annotations,
            [NotNull] IUpdateSqlGenerator sql)
        {
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(executor, nameof(executor));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(sql, nameof(sql));

            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _executor = executor;
            _connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            Sql = sql;

            var relationalOptions = RelationalOptionsExtension.Extract(options);
            TableName = relationalOptions?.MigrationsHistoryTableName ?? DefaultTableName;
            TableSchema = relationalOptions.MigrationsHistoryTableSchema;
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
            var entityType = new LazyRef<IEntityType>(() => _model.Value.GetEntityType(typeof(HistoryRow)));
            _migrationIdColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.MigrationId))).ColumnName);
            _productVersionColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.ProductVersion))).ColumnName);
        }

        protected virtual IUpdateSqlGenerator Sql { get; }
        protected virtual string TableName { get; }
        protected virtual string TableSchema { get; }
        protected virtual string MigrationIdColumnName => _migrationIdColumnName.Value;
        protected virtual string ProductVersionColumnName => _productVersionColumnName.Value;

        protected abstract string ExistsSql { get; }

        public virtual bool Exists()
            => _databaseCreator.Exists() && InterpretExistsResult(_executor.ExecuteScalar(_connection, ExistsSql));

        public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await _databaseCreator.ExistsAsync(cancellationToken)
                && InterpretExistsResult(await _executor.ExecuteScalarAsync(_connection, ExistsSql, cancellationToken));

        /// <returns>true if the table exists; otherwise, false.</returns>
        protected abstract bool InterpretExistsResult([NotNull] object value);

        public abstract string GetCreateIfNotExistsScript();

        public virtual string GetCreateScript()
        {
            var operations = _modelDiffer.GetDifferences(null, _model.Value);
            var commands = _migrationsSqlGenerator.Generate(operations, _model.Value);
            if (commands.Count != 1)
            {
                throw new InvalidOperationException(Strings.InvalidCreateScript);
            }

            return commands[0].CommandText;
        }

        protected virtual void ConfigureTable([NotNull] EntityTypeBuilder<HistoryRow> history)
        {
            history.ToTable(DefaultTableName);
            history.Key(h => h.MigrationId);
            history.Property(h => h.MigrationId).MaxLength(150);
            history.Property(h => h.ProductVersion).MaxLength(32).Required();
        }

        public virtual IReadOnlyList<HistoryRow> GetAppliedMigrations()
        {
            var rows = new List<HistoryRow>();

            if (Exists())
            {
                _connection.Open();
                try
                {
                    using (var reader = _executor.ExecuteReader(_connection, GetAppliedMigrationsSql))
                    {
                        while (reader.Read())
                        {
                            rows.Add(new HistoryRow(reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
                finally
                {
                    _connection.Close();
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
                await _connection.OpenAsync(cancellationToken);
                try
                {
                    using (var reader = await _executor.ExecuteReaderAsync(_connection, GetAppliedMigrationsSql, cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            rows.Add(new HistoryRow(reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
                finally
                {
                    _connection.Close();
                }
            }

            return rows;
        }

        protected virtual string GetAppliedMigrationsSql
            => new StringBuilder()
                .Append("SELECT ")
                .Append(Sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .AppendLine(Sql.DelimitIdentifier(ProductVersionColumnName))
                .Append("FROM ")
                .AppendLine(Sql.DelimitIdentifier(TableName, TableSchema))
                .Append("ORDER BY ")
                .Append(Sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(";")
                .ToString();

        public virtual string GetInsertScript([NotNull] HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(Sql.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(Sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(Sql.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES ('")
                .Append(Sql.EscapeLiteral(row.MigrationId))
                .Append("', '")
                .Append(Sql.EscapeLiteral(row.ProductVersion))
                .AppendLine("');")
                .ToString();
        }

        public virtual string GetDeleteScript([NotNull] string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(Sql.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(Sql.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(Sql.EscapeLiteral(migrationId))
                .AppendLine("';")
                .ToString();
        }

        public abstract string GetBeginIfNotExistsScript(string migrationId);
        public abstract string GetBeginIfExistsScript(string migrationId);
        public abstract string GetEndIfScript();
    }
}
