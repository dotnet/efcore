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
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    // TODO: Leverage query pipeline for GetAppliedMigrations
    // TODO: Leverage update pipeline for GetInsertScript & GetDeleteScript
    public abstract class HistoryRepository : IHistoryRepository
    {
        public const string DefaultTableName = "__EFMigrationsHistory";

        private readonly IRelationalDatabaseCreator _databaseCreator;
        private readonly ISqlCommandBuilder _sqlCommandBuilder;
        private readonly IRelationalConnection _connection;
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly LazyRef<IModel> _model;
        private readonly LazyRef<string> _migrationIdColumnName;
        private readonly LazyRef<string> _productVersionColumnName;

        public HistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] ISqlCommandBuilder sqlCommandBuilder,
            [NotNull] IRelationalConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] ISqlGenerator sqlGenerator)
        {
            Check.NotNull(databaseCreator, nameof(databaseCreator));
            Check.NotNull(sqlCommandBuilder, nameof(sqlCommandBuilder));
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));
            Check.NotNull(modelDiffer, nameof(modelDiffer));
            Check.NotNull(migrationsSqlGenerator, nameof(migrationsSqlGenerator));
            Check.NotNull(annotations, nameof(annotations));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));

            _databaseCreator = (IRelationalDatabaseCreator)databaseCreator;
            _sqlCommandBuilder = sqlCommandBuilder;
            _connection = connection;
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            SqlGenerator = sqlGenerator;

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
            var entityType = new LazyRef<IEntityType>(() => _model.Value.FindEntityType(typeof(HistoryRow)));
            _migrationIdColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.MigrationId))).ColumnName);
            _productVersionColumnName = new LazyRef<string>(
                () => annotations.For(entityType.Value.FindProperty(nameof(HistoryRow.ProductVersion))).ColumnName);
        }

        protected virtual ISqlGenerator SqlGenerator { get; }
        protected virtual string TableName { get; }
        protected virtual string TableSchema { get; }
        protected virtual string MigrationIdColumnName => _migrationIdColumnName.Value;
        protected virtual string ProductVersionColumnName => _productVersionColumnName.Value;

        protected abstract string ExistsSql { get; }

        public virtual bool Exists()
            => _databaseCreator.Exists()
                && InterpretExistsResult(
                    _sqlCommandBuilder.Build(ExistsSql).ExecuteScalar(_connection));

        public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await _databaseCreator.ExistsAsync(cancellationToken)
                && InterpretExistsResult(
                    await _sqlCommandBuilder.Build(ExistsSql).ExecuteScalarAsync(_connection, cancellationToken));

        /// <returns>true if the table exists; otherwise, false.</returns>
        protected abstract bool InterpretExistsResult([NotNull] object value);

        public abstract string GetCreateIfNotExistsScript();

        public virtual string GetCreateScript()
        {
            var operations = _modelDiffer.GetDifferences(null, _model.Value);
            var commands = _migrationsSqlGenerator.Generate(operations, _model.Value);
            if (commands.Count != 1)
            {
                throw new InvalidOperationException(RelationalStrings.InvalidCreateScript);
            }

            return commands[0].CommandText;
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
                var command = _sqlCommandBuilder.Build(GetAppliedMigrationsSql);

                using (var reader = command.ExecuteReader(_connection))
                {
                    while (reader.DbDataReader.Read())
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
                var command = _sqlCommandBuilder.Build(GetAppliedMigrationsSql);

                using (var reader = await command.ExecuteReaderAsync(_connection))
                {
                    while (await reader.DbDataReader.ReadAsync(cancellationToken))
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
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .AppendLine(SqlGenerator.DelimitIdentifier(ProductVersionColumnName))
                .Append("FROM ")
                .AppendLine(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append("ORDER BY ")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(";")
                .ToString();

        public virtual string GetInsertScript([NotNull] HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(SqlGenerator.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES ('")
                .Append(SqlGenerator.EscapeLiteral(row.MigrationId))
                .Append("', '")
                .Append(SqlGenerator.EscapeLiteral(row.ProductVersion))
                .AppendLine("');")
                .ToString();
        }

        public virtual string GetDeleteScript([NotNull] string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(SqlGenerator.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(SqlGenerator.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(SqlGenerator.EscapeLiteral(migrationId))
                .AppendLine("';")
                .ToString();
        }

        public abstract string GetBeginIfNotExistsScript(string migrationId);
        public abstract string GetBeginIfExistsScript(string migrationId);
        public abstract string GetEndIfScript();
    }
}
